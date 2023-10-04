using UnityEngine;
using System.IO.Ports;
using DynamixelForUnity;
using UnityEditor.Experimental.GraphView;
using UnityEditor.VersionControl;
using System;
using System.Collections;

namespace DynamixelForUnity.Demo
{
    public class SimpleDynamixelController : MonoBehaviour
    {
        [SerializeField] private byte id = 1;
        [SerializeField, Range(0, 4095)] private uint goal_angle = 1000;
        [SerializeField] private bool torque_on = true;
        [SerializeField] private mode operating = mode.Position;
        [SerializeField] private string portName = "COM4"; // �ڑ�����V���A���|�[�g�̖��O���w��
        [SerializeField] private int baudRate = 57600; // �{�[���[�g���w��

        public enum mode
        {
            Current,
            Velocity,
            None,
            Position,
            ExtendedPosition,
            CurrentBasePosition,
            PWM
        }

        Dynamixel dynamixel = new Dynamixel(false);
        private SerialPort serialPort;

        void Start()
        {
            //Project Settings > Player > Other Settings > Api Compatibility Level���u.NET Framework�v�ɕύX
            serialPort = new SerialPort(portName, baudRate);
            serialPort.Open();
        }

        [ContextMenu("operating_mode")]
        public void operating_mode()
        {
            byte[] dataByte = dynamixel.OperatingMode(id, (byte)operating);
            serialPort.Write(dataByte, 0, dataByte.Length);

            StartCoroutine("ReadData");
        }

        [ContextMenu("torque_enable")]
        public void torque_enable()
        {
            byte[] dataByte;
            if (torque_on) dataByte = dynamixel.TorqueEnable(id, 1);
            else dataByte = dynamixel.TorqueEnable(id, 0);
            serialPort.Write(dataByte, 0, dataByte.Length);

            StartCoroutine("ReadData");
        }

        [ContextMenu("goal_position")]
        public void goal_position()
        {
            byte[] dataByte = dynamixel.GoalPosition(id, goal_angle);
            serialPort.Write(dataByte, 0, dataByte.Length);

            StartCoroutine("ReadData");
        }

        [ContextMenu("present_position")]
        public void present_position()
        {
            byte[] dataByte = dynamixel.PresentPosition(id);
            serialPort.Write(dataByte, 0, dataByte.Length);

            StartCoroutine("ReadData");
        }

        IEnumerator ReadData()
        {
            yield return new WaitForSeconds(0.5f);

            // �o�C�g�f�[�^��ǂݎ���
            if (serialPort.BytesToRead > 0)
            {
                byte[] buffer = new byte[serialPort.BytesToRead];
                serialPort.Read(buffer, 0, buffer.Length);

                //paramater������ꍇ
                byte[] paramater = dynamixel.GetParameterFromPacket(buffer);
                if (paramater.Length > 0) Debug.Log($"Paramater: {BitConverter.ToUInt16(paramater, 0)}");
            }
        }

        void OnDestroy()
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.Close(); // �A�v���P�[�V�������I�������ۂɃV���A���|�[�g�����
            }
        }
    }
}