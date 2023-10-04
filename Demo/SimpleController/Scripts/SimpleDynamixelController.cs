using System;
using UnityEngine;

namespace DynamixelForUnity.Demo
{
    public class SimpleDynamixelController : MonoBehaviour
    {
        [SerializeField] private byte id = 1;
        [SerializeField, Range(0, 4095)]
        private uint goal_angle = 1000;
        [SerializeField] private bool torque_on = true;
        [SerializeField] private mode operating = mode.Position;

        Dynamixel dynamixel = new Dynamixel(false);
        SerialPortUtility.SerialPortUtilityPro port;

        public enum mode
        {
            Current,
            Velocity,
            Position,
            ExtendedPosition,
            CurrentBasePosition,
            PWM
        }

        private void Start()
        {
            port = this.GetComponent<SerialPortUtility.SerialPortUtilityPro>();
        }

        [ContextMenu("operating_mode")]
        public void operating_mode()
        {
            byte[] dataByte = dynamixel.OperatingMode(id, (byte)operating);
            port.Write(dataByte);
        }

        [ContextMenu("torque_enable")]
        public void torque_enable()
        {
            byte[] dataByte;
            if (torque_on) dataByte = dynamixel.TorqueEnable(id, 1);
            else dataByte = dynamixel.TorqueEnable(id, 0);
            port.Write(dataByte);
        }

        [ContextMenu("goal_position")]
        public void goal_position()
        {
            byte[] dataByte = dynamixel.GoalPosition(id, goal_angle);
            port.Write(dataByte);
        }

        [ContextMenu("present_position")]
        public void present_position()
        {
            byte[] dataByte = dynamixel.PresentPosition(id);
            port.Write(dataByte);
        }

        /// <summary>
        /// Dynamixelからのデータ読み取りメソッド
        /// </summary>
        /// <param name="data"></param>
        public void ReadComprateList(object data)
        {
            if (data is byte[])
            {
                byte[] byteData = (byte[])data;
                //string dataText = "[";
                //foreach (byte b in byteData) dataText += $"{b.ToString()}, ";
                //dataText = dataText.Substring(0, dataText.Length - 2) + "]";
                //Debug.Log("Read All Data: " + dataText);

                byte[] paramater = dynamixel.GetParameterFromPacket(byteData);
                if (paramater.Length > 0) Debug.Log($"Paramater: {BitConverter.ToUInt16(paramater, 0)}");
            }
        }
    }
}