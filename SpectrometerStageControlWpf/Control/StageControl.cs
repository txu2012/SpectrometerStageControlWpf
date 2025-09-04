using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Thorlabs.MotionControl.DeviceManagerCLI;
using Thorlabs.MotionControl.GenericMotorCLI;
using Thorlabs.MotionControl.GenericMotorCLI.ControlParameters;
using Thorlabs.MotionControl.GenericMotorCLI.AdvancedMotor;
using Thorlabs.MotionControl.GenericMotorCLI.KCubeMotor;
using Thorlabs.MotionControl.GenericMotorCLI.Settings;
using Thorlabs.MotionControl.KCube.DCServoCLI;

namespace SpectrometerStageControlWpf
{
    public enum MotorState 
    {
        Stopped,
        Moving,
        Homing
    }

    public class StageControl : IDisposable
    {
        #region Class Members
        private string serialNumber_;
        private KCubeDCServo device_;
        private decimal currentPosition_;
        private MotorState state_;

        public decimal CurrentPosition 
        { 
            get 
            { 
                return currentPosition_; 
            }
            set
            {
                this.currentPosition_ = value;
            }
        }

        public decimal CurrentPosition_mm
        {
            get
            {
                var stepsPerMm = 34555;
                var mmPerStep = Decimal.Divide(1, stepsPerMm);
                return this.currentPosition_ * mmPerStep;
            }
        }
        public bool IsConnected
        {
            get => (device_ != null) ? device_.IsConnected : false; 
        }
        public bool IsHomed
        {
            get => (device_ != null) ? device_.Status.IsHomed : false;
        }
        public MotorState StageState { get { return state_; } }
        #endregion

        #region Dispose pattern
        ~StageControl()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        private bool disposed = false;
        #endregion

        public StageControl() 
        {
            currentPosition_ = 0;
            state_ = MotorState.Stopped;
        }

        public void Connect(string serialNumber)
        {
            try
            {
                if (!checkDeviceList(serialNumber))
                    throw new InvalidOperationException("Device does not exist.");

                serialNumber_ = serialNumber;
                device_ = KCubeDCServo.CreateKCubeDCServo(serialNumber_);

                // Connect to device
                try
                {
                    Console.WriteLine($"Opening {serialNumber} device.");
                    device_.Connect(serialNumber_);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to open device {serialNumber}");
                    throw ex;
                }

                if (!device_.IsSettingsInitialized())
                {
                    try
                    {
                        Console.WriteLine("Initializing Settings");
                        device_.WaitForSettingsInitialized(5000);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Settings failed to initialize. {ex.StackTrace}");
                        throw ex;
                    }
                }

                // Start polling
                device_.StartPolling(250);
                Thread.Sleep(500);

                // Enable the device
                device_.EnableDevice();
                Thread.Sleep(500);

                initialize();
                taskComplete_ = true;

                GetStatus();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating DeviceControl: {ex.Message}");
                throw;
            }
        }

        public void Disconnect() 
        {
            device_.StopPolling();
            device_.Disconnect(true);
            device_.ShutDown();
            device_ = null;
        }

        public void Home() 
        {
            try
            {
                if (IsConnected && taskComplete_ && state_ == MotorState.Stopped)
                {
                    Console.WriteLine("Homing device");

                    taskComplete_ = false;
                    taskID_ = device_.Home(CommandCompleteFunction);
                    state_ = MotorState.Homing;
                }
            }
            catch (Exception ex)
            {
                taskComplete_ = true;
                state_ = MotorState.Stopped;
            }
        }

        public void MoveTo(decimal posMm) 
        {
            try
            {
                if (IsConnected && taskComplete_ && state_ == MotorState.Stopped)
                {
                    Console.WriteLine($"Moving Device to {posMm} mm");
                    taskComplete_ = false;
                    taskID_ = device_.MoveTo(posMm, CommandCompleteFunction);
                    state_ = MotorState.Moving;
                }
            }
            catch (Exception ex)
            {
                taskComplete_ = true;
                state_ = MotorState.Stopped;
            }
        }

        public void MoveRelative(bool fwd, decimal amountMm)
        {
            try
            {
                if (IsConnected && taskComplete_ && state_ == MotorState.Stopped)
                {
                    MotorDirection direction = (fwd) ? MotorDirection.Forward : MotorDirection.Backward;
                    Console.WriteLine($"Moving device {amountMm} mm {direction.ToString()}");
                    taskComplete_ = false;
                    taskID_ = device_.MoveRelative(direction, amountMm, CommandCompleteFunction);
                    state_ = MotorState.Moving;
                }
            }
            catch (Exception ex)
            {
                taskComplete_ = true;
                state_ = MotorState.Stopped;
            }
        }

        public void MoveContiuous(bool fwd)
        {
            try
            {
                if (IsConnected && taskComplete_ && state_ == MotorState.Stopped)
                {
                    MotorDirection direction = (fwd) ? MotorDirection.Forward : MotorDirection.Backward;
                    Console.WriteLine($"Moving Device in {direction.ToString()}");
                    taskComplete_ = false;
                    taskID_ = device_.MoveContinuous(direction);
                    state_ = MotorState.Moving;
                }
            }
            catch (Exception ex)
            {
                taskComplete_ = true;
                state_ = MotorState.Stopped;
            }
        }

        public void Stop(bool immediate = false)
        {
            if (IsConnected)
            {
                Console.WriteLine($"Stopping Device");

                if (immediate)
                {
                    device_.StopImmediate();
                    state_ = MotorState.Stopped;
                }
                else
                {
                    taskComplete_ = false;
                    taskID_ = device_.Stop(CommandCompleteFunction);
                }
            }
        }

        public void GetStatus()
        {
            if (IsConnected)
            {
                StatusBase status = device_.Status;
                if (taskComplete_)
                {
                    
                    if (state_ == MotorState.Homing && !status.IsInMotion)
                    {
                        state_ = MotorState.Stopped;
                    }
                    else if (state_ == MotorState.Moving && !status.IsInMotion)
                    {
                        state_ = MotorState.Stopped;
                    }
                    else
                    {
                        state_ = MotorState.Stopped;
                    }
                }
                currentPosition_ = status.Position;
            }
        }

        public List<string> GetDevices()
        {
            try
            {
                // Tell the device manager to get the list of all devices connected to the computer
                DeviceManagerCLI.BuildDeviceList();
                return DeviceManagerCLI.GetDeviceList(KCubeDCServo.DevicePrefix);
            }
            catch (Exception ex)
            {
                // An error occurred - see ex for details
                Console.WriteLine($"Exception raised by BuildDeviceList {ex}");
                throw ex;
            }
        }

        #region Private Functions
        private bool checkDeviceList(string sn)
        {
            try
            {
                // Tell the device manager to get the list of all devices connected to the computer
                DeviceManagerCLI.BuildDeviceList();

                List<string> serialNumbers = DeviceManagerCLI.GetDeviceList(KCubeDCServo.DevicePrefix);
                if (!serialNumbers.Contains(sn))
                {
                    Console.WriteLine($"{sn} is not a valid serial number");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                // An error occurred - see ex for details
                Console.WriteLine($"Exception raised by BuildDeviceList {ex}");
                return false;
            }
        }

        private void initialize()
        {
            MotorConfiguration motorConfiguration = device_.LoadMotorConfiguration(serialNumber_);
            motorConfiguration.DeviceSettingsName = "MTS25";
            motorConfiguration.UpdateCurrentConfiguration();
        }
        #endregion

        #region Task Checker
        private bool taskComplete_ = true;
        private ulong taskID_;
        private void CommandCompleteFunction(ulong taskID)
        {
            GetStatus();

            if ((taskID_ > 0) && (taskID_ == taskID))
            {
                taskComplete_ = true;
                GetStatus();
                Console.WriteLine($"Finished task. {state_}");
            }
        }
        #endregion
    }
}
