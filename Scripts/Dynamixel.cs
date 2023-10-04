using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DynamixelForUnity
{
    public class Dynamixel
    {
        private const byte READ_DATA = 2;       // データの読み込み
        private const byte WRITE_DATA = 3;      // データの書き込み
        private const byte SYNC_WRITE = 0x83;   // データの一括書き込み
        private const byte SYNC_READ = 0x82;    // データの一括読み込み
        private bool showLog = false;

        /// <summary>
        /// 送ったデータの詳細を見る場合はtrue
        /// </summary>
        /// <param name="showLog"></param>
        public Dynamixel(bool showLog)
        {
            this.showLog = showLog;
        }

        /// <summary>
        /// make_crc16_table
        /// </summary>
        /// <returns></returns>
        public ushort[] MakeCrc16Table()
        {
            ushort[] crc16Table = new ushort[]
            {
            0x0000, 0x8005, 0x800F, 0x000A, 0x801B, 0x001E, 0x0014, 0x8011,
            0x8033, 0x0036, 0x003C, 0x8039, 0x0028, 0x802D, 0x8027, 0x0022,
            0x8063, 0x0066, 0x006C, 0x8069, 0x0078, 0x807D, 0x8077, 0x0072,
            0x0050, 0x8055, 0x805F, 0x005A, 0x804B, 0x004E, 0x0044, 0x8041,
            0x80C3, 0x00C6, 0x00CC, 0x80C9, 0x00D8, 0x80DD, 0x80D7, 0x00D2,
            0x00F0, 0x80F5, 0x80FF, 0x00FA, 0x80EB, 0x00EE, 0x00E4, 0x80E1,
            0x00A0, 0x80A5, 0x80AF, 0x00AA, 0x80BB, 0x00BE, 0x00B4, 0x80B1,
            0x8093, 0x0096, 0x009C, 0x8099, 0x0088, 0x808D, 0x8087, 0x0082,
            0x8183, 0x0186, 0x018C, 0x8189, 0x0198, 0x819D, 0x8197, 0x0192,
            0x01B0, 0x81B5, 0x81BF, 0x01BA, 0x81AB, 0x01AE, 0x01A4, 0x81A1,
            0x01E0, 0x81E5, 0x81EF, 0x01EA, 0x81FB, 0x01FE, 0x01F4, 0x81F1,
            0x81D3, 0x01D6, 0x01DC, 0x81D9, 0x01C8, 0x81CD, 0x81C7, 0x01C2,
            0x0140, 0x8145, 0x814F, 0x014A, 0x815B, 0x015E, 0x0154, 0x8151,
            0x8173, 0x0176, 0x017C, 0x8179, 0x0168, 0x816D, 0x8167, 0x0162,
            0x8123, 0x0126, 0x012C, 0x8129, 0x0138, 0x813D, 0x8137, 0x0132,
            0x0110, 0x8115, 0x811F, 0x011A, 0x810B, 0x010E, 0x0104, 0x8101,
            0x8303, 0x0306, 0x030C, 0x8309, 0x0318, 0x831D, 0x8317, 0x0312,
            0x0330, 0x8335, 0x833F, 0x033A, 0x832B, 0x032E, 0x0324, 0x8321,
            0x0360, 0x8365, 0x836F, 0x036A, 0x837B, 0x037E, 0x0374, 0x8371,
            0x8353, 0x0356, 0x035C, 0x8359, 0x0348, 0x834D, 0x8347, 0x0342,
            0x03C0, 0x83C5, 0x83CF, 0x03CA, 0x83DB, 0x03DE, 0x03D4, 0x83D1,
            0x83F3, 0x03F6, 0x03FC, 0x83F9, 0x03E8, 0x83ED, 0x83E7, 0x03E2,
            0x83A3, 0x03A6, 0x03AC, 0x83A9, 0x03B8, 0x83BD, 0x83B7, 0x03B2,
            0x0390, 0x8395, 0x839F, 0x039A, 0x838B, 0x038E, 0x0384, 0x8381,
            0x0280, 0x8285, 0x828F, 0x028A, 0x829B, 0x029E, 0x0294, 0x8291,
            0x82B3, 0x02B6, 0x02BC, 0x82B9, 0x02A8, 0x82AD, 0x82A7, 0x02A2,
            0x82E3, 0x02E6, 0x02EC, 0x82E9, 0x02F8, 0x82FD, 0x82F7, 0x02F2,
            0x02D0, 0x82D5, 0x82DF, 0x02DA, 0x82CB, 0x02CE, 0x02C4, 0x82C1,
            0x8243, 0x0246, 0x024C, 0x8249, 0x0258, 0x825D, 0x8257, 0x0252,
            0x0270, 0x8275, 0x827F, 0x027A, 0x826B, 0x026E, 0x0264, 0x8261,
            0x0220, 0x8225, 0x822F, 0x022A, 0x823B, 0x023E, 0x0234, 0x8231,
            0x8213, 0x0216, 0x021C, 0x8219, 0x0208, 0x820D, 0x8207, 0x0202
            };

            return crc16Table;
        }

        /// <summary>
        /// crc16
        /// </summary>
        /// <param name="checksum"></param>
        /// <returns></returns>
        public ushort CalculateCrc16(ushort[] checksum)
        {
            ushort[] table = MakeCrc16Table();
            ushort crc_accum = 0;
            int data_blk_size = checksum.Length;

            for (int j = 0; j < data_blk_size; j++)
            {
                crc_accum = (ushort)(crc_accum & 0xFFFF);
                int i = ((crc_accum >> 8) ^ checksum[j]) & 0xFF;
                crc_accum = (ushort)((crc_accum << 8) ^ table[i]);
            }

            return crc_accum;
        }

        /// <summary>
        /// 0   Current Control Mode, 
        /// 1   Velocity Control Mode, 
        /// 3   Position Control Mode, 
        /// 4   Extended Position Control Mode, 
        /// 5   Current-Base Position Control Mode, 
        /// 16  PWM Control Mode, 
        /// ※出力軸ロック時は使用不可
        /// Parameters
        /// ----------
        /// ids : array-like of int
        /// [id1, id2, id3, ...]
        /// modes : array-like of int
        /// [mode1, mode2, mode3, ...]
        /// </summary>
        /// <param name="id"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public byte[] OperatingMode(byte id, byte mode)
        {
            byte[] data = new byte[] { 0xFF, 0xFF, 0xFD, 0x00, id, 0, 0, WRITE_DATA, 11, 0, mode, 0, 0 };
            data[5] = (byte)((data.Length - 7) % 256);
            data[6] = (byte)((data.Length - 7) >> 8);

            ushort[] checksum = new ushort[data.Length - 2];
            Array.Copy(data, checksum, checksum.Length);

            ushort crc = CalculateCrc16(checksum);
            data[data.Length - 2] = (byte)(crc & 0xff);
            data[data.Length - 1] = (byte)(crc >> 8);

            if (showLog) LogDataAndChecksum(data, checksum, crc);

            return data;
        }

        /// <summary>
        /// sync_operating_mode
        /// 動作未確認
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="modes"></param>
        /// <returns></returns>
        public byte[] SyncOperatingMode(List<byte> ids, List<byte> modes)
        {
            List<byte> data = new List<byte>
        {
            0xFF, 0xFF, 0xFD, 0x00, 0xFE, 0x00, 0x00, SYNC_WRITE, 11, 0, 0x01, 0x00
        };

            for (int i = 0; i < ids.Count; i++)
            {
                data.Add(ids[i]);
                data.Add(modes[i]);
            }

            data[5] = (byte)((data.Count + 2 - 7) & 0xFF);
            data[6] = (byte)((data.Count + 2 - 7) >> 8);

            ushort[] checksum = new ushort[data.Count - 2];
            Array.Copy(data.ToArray(), checksum, checksum.Length);

            ushort crc = CalculateCrc16(checksum);
            data.Add((byte)(crc & 0xFF));
            data.Add((byte)(crc >> 8));

            byte[] dataArray = data.ToArray();

            if (showLog) LogDataAndChecksum(dataArray, checksum, crc);

            return dataArray;
        }

        /// <summary>
        /// torque_enable
        /// </summary>
        /// <param name="id"></param>
        /// <param name="switchValue"></param>
        /// <returns></returns>
        public byte[] TorqueEnable(byte id, byte switchValue)
        {
            byte[] data = new byte[] { 0xFF, 0xFF, 0xFD, 0x00, id, 0, 0, WRITE_DATA, 64, 0, switchValue, 0, 0 };
            data[5] = (byte)((data.Length - 7) % 256);
            data[6] = (byte)((data.Length - 7) >> 8);

            ushort[] checksum = new ushort[data.Length - 2];
            Array.Copy(data, checksum, checksum.Length);

            ushort crc = CalculateCrc16(checksum);
            data[data.Length - 2] = (byte)(crc & 0xff);
            data[data.Length - 1] = (byte)(crc >> 8);

            if (showLog) LogDataAndChecksum(data, checksum, crc);

            return data;
        }

        /// <summary>
        /// sync_torque_enable
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="switches"></param>
        /// <returns></returns>
        public byte[] SyncTorqueEnable(List<byte> ids, List<byte> switches)
        {
            List<byte> data = new List<byte>
        {
            0xFF, 0xFF, 0xFD, 0x00, 0xFE, 0x00, 0x00, SYNC_WRITE, 64, 0, 0x01, 0x00
        };

            for (int i = 0; i < ids.Count; i++)
            {
                data.Add(ids[i]);
                data.Add(switches[i]);
            }

            data[5] = (byte)((data.Count + 2 - 7) & 0xFF);
            data[6] = (byte)((data.Count + 2 - 7) >> 8);

            ushort[] checksum = new ushort[data.Count - 2];
            Array.Copy(data.ToArray(), checksum, checksum.Length);

            ushort crc = CalculateCrc16(checksum);
            data.Add((byte)(crc & 0xFF));
            data.Add((byte)(crc >> 8));

            byte[] dataArray = data.ToArray();

            if (showLog) LogDataAndChecksum(dataArray, checksum, crc);

            return dataArray;
        }

        /// <summary>
        /// max_position_limit
        /// </summary>
        /// <param name="id"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        public byte[] MaxPositionLimit(byte id, uint angle)
        {
            byte[] angleBytes = BitConverter.GetBytes(angle);
            List<byte> data = new List<byte>
        {
            0xFF, 0xFF, 0xFD, 0x00, id, 0, 0, WRITE_DATA, 48, 0, angleBytes[0], angleBytes[1], angleBytes[2], angleBytes[3], 0, 0
        };

            data[5] = (byte)((data.Count + 2 - 7) & 0xFF);
            data[6] = (byte)((data.Count + 2 - 7) >> 8);

            ushort[] checksum = new ushort[data.Count - 2];
            for (int i = 0; i < checksum.Length; i++)
            {
                checksum[i] = data[i];
            }

            ushort crc = CalculateCrc16(checksum);
            data.Add((byte)(crc & 0xFF));
            data.Add((byte)(crc >> 8));

            byte[] dataArray = data.ToArray();

            if (showLog) LogDataAndChecksum(dataArray, checksum, crc);

            return dataArray;
        }

        /// <summary>
        /// min_position_limit
        /// </summary>
        /// <param name="id"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        public byte[] MinPositionLimit(byte id, uint angle)
        {
            byte[] angleBytes = BitConverter.GetBytes(angle);
            List<byte> data = new List<byte>
        {
            0xFF, 0xFF, 0xFD, 0x00, id, 0, 0, WRITE_DATA, 52, 0, angleBytes[0], angleBytes[1], angleBytes[2], angleBytes[3], 0, 0
        };

            data[5] = (byte)((data.Count + 2 - 7) & 0xFF);
            data[6] = (byte)((data.Count + 2 - 7) >> 8);

            ushort[] checksum = new ushort[data.Count - 2];
            for (int i = 0; i < checksum.Length; i++)
            {
                checksum[i] = data[i];
            }

            ushort crc = CalculateCrc16(checksum);
            data.Add((byte)(crc & 0xFF));
            data.Add((byte)(crc >> 8));

            byte[] dataArray = data.ToArray();

            if (showLog) LogDataAndChecksum(dataArray, checksum, crc);

            return dataArray;
        }

        /// <summary>
        /// position_D_gain
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dGain"></param>
        /// <returns></returns>
        public byte[] SetPositionDGain(byte id, ushort dGain)
        {
            byte[] dGainBytes = BitConverter.GetBytes(dGain);
            List<byte> data = new List<byte>
    {
        0xFF, 0xFF, 0xFD, 0x00, id, 0, 0, WRITE_DATA, 80, 0, dGainBytes[0], dGainBytes[1], 0, 0
    };

            data[5] = (byte)((data.Count + 2 - 7) & 0xFF);
            data[6] = (byte)((data.Count + 2 - 7) >> 8);

            ushort[] checksum = new ushort[data.Count - 2];
            for (int i = 0; i < checksum.Length; i++)
            {
                checksum[i] = data[i];
            }

            ushort crc = CalculateCrc16(checksum);
            data.Add((byte)(crc & 0xFF));
            data.Add((byte)(crc >> 8));

            byte[] dataArray = data.ToArray();

            if (showLog) LogDataAndChecksum(dataArray, checksum, crc);

            return dataArray;
        }

        /// <summary>
        /// position_I_gain
        /// </summary>
        /// <param name="id"></param>
        /// <param name="iGain"></param>
        /// <returns></returns>
        public byte[] SetPositionIGain(byte id, ushort iGain)
        {
            byte[] iGainBytes = BitConverter.GetBytes(iGain);
            List<byte> data = new List<byte>
        {
            0xFF, 0xFF, 0xFD, 0x00, id, 0, 0, WRITE_DATA, 82, 0, iGainBytes[0], iGainBytes[1], 0, 0
        };

            data[5] = (byte)((data.Count + 2 - 7) & 0xFF);
            data[6] = (byte)((data.Count + 2 - 7) >> 8);

            ushort[] checksum = new ushort[data.Count - 2];
            for (int i = 0; i < checksum.Length; i++)
            {
                checksum[i] = data[i];
            }

            ushort crc = CalculateCrc16(checksum);
            data.Add((byte)(crc & 0xFF));
            data.Add((byte)(crc >> 8));

            byte[] dataArray = data.ToArray();

            if (showLog) LogDataAndChecksum(dataArray, checksum, crc);

            return dataArray;
        }

        /// <summary>
        /// position_P_gain
        /// </summary>
        /// <param name="id"></param>
        /// <param name="pGain"></param>
        /// <returns></returns>
        public byte[] SetPositionPGain(byte id, ushort pGain)
        {
            byte[] pGainBytes = BitConverter.GetBytes(pGain);
            List<byte> data = new List<byte>
        {
            0xFF, 0xFF, 0xFD, 0x00, id, 0, 0, WRITE_DATA, 84, 0, pGainBytes[0], pGainBytes[1], 0, 0
        };

            data[5] = (byte)((data.Count + 2 - 7) & 0xFF);
            data[6] = (byte)((data.Count + 2 - 7) >> 8);

            ushort[] checksum = new ushort[data.Count - 2];
            for (int i = 0; i < checksum.Length; i++)
            {
                checksum[i] = data[i];
            }

            ushort crc = CalculateCrc16(checksum);
            data.Add((byte)(crc & 0xFF));
            data.Add((byte)(crc >> 8));

            byte[] dataArray = data.ToArray();

            if (showLog) LogDataAndChecksum(dataArray, checksum, crc);

            return dataArray;
        }

        /// <summary>
        /// goal_velocity
        /// </summary>
        /// <param name="id"></param>
        /// <param name="speed"></param>
        /// <returns></returns>
        public byte[] SetGoalVelocity(byte id, uint speed)
        {
            byte[] speedBytes = BitConverter.GetBytes(speed);
            byte[] data = new byte[]
            {
            0xFF, 0xFF, 0xFD, 0x00, id, 0, 0, WRITE_DATA, 104, 0, speedBytes[0], speedBytes[1], speedBytes[2], speedBytes[3], 0, 0
            };
            data[5] = (byte)((data.Length - 7) % 256);
            data[6] = (byte)((data.Length - 7) >> 8);

            ushort[] checksum = new ushort[data.Length - 2];
            Array.Copy(data, checksum, checksum.Length);

            ushort crc = CalculateCrc16(checksum);
            data[data.Length - 2] = (byte)(crc & 0xff);
            data[data.Length - 1] = (byte)(crc >> 8);

            if (showLog) LogDataAndChecksum(data, checksum, crc);

            return data;
        }

        /// <summary>
        /// sync_goal_velocity
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="speeds"></param>
        /// <returns></returns>
        public byte[] SyncGoalVelocity(List<byte> ids, List<uint> speeds)
        {
            List<byte> data = new List<byte>
        {
            0xFF, 0xFF, 0xFD, 0x00, 0xFE, 0x00, 0x00, SYNC_WRITE, 104, 0, 0x04, 0x00
        };

            for (int i = 0; i < ids.Count; i++)
            {
                byte[] speedBytes = BitConverter.GetBytes(speeds[i]);
                data.Add(ids[i]);
                data.Add(speedBytes[0]);
                data.Add(speedBytes[1]);
                data.Add(speedBytes[2]);
                data.Add(speedBytes[3]);
            }

            data[5] = (byte)((data.Count + 2 - 7) & 0xFF);
            data[6] = (byte)((data.Count + 2 - 7) >> 8);

            ushort[] checksum = new ushort[data.Count - 2];
            Array.Copy(data.ToArray(), checksum, checksum.Length);

            ushort crc = CalculateCrc16(checksum);
            data.Add((byte)(crc & 0xFF));
            data.Add((byte)(crc >> 8));

            byte[] dataArray = data.ToArray();

            if (showLog) LogDataAndChecksum(dataArray, checksum, crc);

            return dataArray;
        }

        /// <summary>
        /// goal_position
        /// </summary>
        /// <param name="id"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        public byte[] GoalPosition(byte id, uint angle)
        {
            byte[] data = new byte[] { 0xFF, 0xFF, 0xFD, 0x00, id, 0, 0, WRITE_DATA, 116, 0, (byte)(angle & 0xff), (byte)(angle >> 8 & 0xff), (byte)(angle >> 16 & 0xff), (byte)(angle >> 24), 0, 0 };
            data[5] = (byte)((data.Length - 7) % 256);
            data[6] = (byte)((data.Length - 7) >> 8);

            ushort[] checksum = new ushort[data.Length - 2];
            Array.Copy(data, checksum, checksum.Length);

            ushort crc = CalculateCrc16(checksum);
            data[data.Length - 2] = (byte)(crc & 0xff);
            data[data.Length - 1] = (byte)(crc >> 8);

            if (showLog) LogDataAndChecksum(data, checksum, crc);

            return data;
        }

        /// <summary>
        /// sync_goal_position
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="angles"></param>
        /// <returns></returns>
        public byte[] SyncGoalPosition(List<byte> ids, List<int> angles)
        {
            List<byte> data = new List<byte>
    {
        0xFF, 0xFF, 0xFD, 0x00, 0xFE, 0x00, 0x00, SYNC_WRITE, 116, 0, 0x04, 0x00
    };

            for (int i = 0; i < ids.Count; i++)
            {
                byte[] angleBytes = BitConverter.GetBytes(angles[i]);
                data.Add(ids[i]);
                data.Add(angleBytes[0]);
                data.Add(angleBytes[1]);
                data.Add(angleBytes[2]);
                data.Add(angleBytes[3]);
            }

            data[5] = (byte)((data.Count + 2 - 7) & 0xFF);
            data[6] = (byte)((data.Count + 2 - 7) >> 8);

            ushort[] checksum = new ushort[data.Count - 2];
            Array.Copy(data.ToArray(), checksum, checksum.Length);

            ushort crc = CalculateCrc16(checksum);
            data.Add((byte)(crc & 0xFF));
            data.Add((byte)(crc >> 8));

            LogDataAndChecksum(data.ToArray(), checksum, crc);

            return data.ToArray();
        }

        /// <summary>
        /// present_velocity
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public byte[] PresentVelocity(byte id)
        {
            byte[] data = new byte[]
            {
            0xFF, 0xFF, 0xFD, 0x00, id, 0, 0, READ_DATA, 128, 0, 0x04, 0x00, 0, 0
            };
            data[5] = (byte)((data.Length - 7) % 256);
            data[6] = (byte)((data.Length - 7) >> 8);

            ushort[] checksum = new ushort[data.Length - 2];
            Array.Copy(data, checksum, checksum.Length);

            ushort crc = CalculateCrc16(checksum);
            data[12] = (byte)(crc & 0xff);
            data[13] = (byte)(crc >> 8);

            if (showLog) LogDataAndChecksum(data, checksum, crc);

            return data;

            //byte[] response = SendCommand(inf);
            //return BitConverter.ToInt32(response, 5);
        }

        /// <summary>
        /// sync_present_velocity
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public byte[] SyncPresentVelocity(List<byte> ids)
        {
            byte[] data = new byte[]
            {
            0xFF, 0xFF, 0xFD, 0x00, 0xFE, 0x00, 0x00, SYNC_READ, 128, 0, 0x04, 0x00
            };
            foreach (byte id in ids)
            {
                data = data.Concat(new byte[] { id, 0, 0, 0, 0 }).ToArray();
            }
            data[5] = (byte)((data.Length + 2 - 7) & 0xFF);
            data[6] = (byte)((data.Length + 2 - 7) >> 8);

            ushort[] checksum = new ushort[data.Length - 2];
            Array.Copy(data, checksum, checksum.Length);

            ushort crc = CalculateCrc16(checksum);
            data = data.Concat(new byte[] { (byte)(crc & 0xff), (byte)(crc >> 8) }).ToArray();

            if (showLog) LogDataAndChecksum(data, checksum, crc);

            return data;

            //byte[] response = SendCommand(inf);

            //Dictionary<byte, int> velocities = new Dictionary<byte, int>();
            //int index = 5;
            //for (int i = 0; i < ids.Count; i++)
            //{
            //    velocities[ids[i]] = BitConverter.ToInt32(response, index);
            //    index += 4;
            //}
            //return velocities;
        }

        /// <summary>
        /// present_position
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public byte[] PresentPosition(byte id)
        {
            byte[] data = new byte[]
            {
            0xFF, 0xFF, 0xFD, 0x00, id, 0, 0, READ_DATA, 132, 0, 0x04, 0x00, 0, 0
            };
            data[5] = (byte)((data.Length - 7) % 256);
            data[6] = (byte)((data.Length - 7) >> 8);

            ushort[] checksum = new ushort[data.Length - 2];
            Array.Copy(data, checksum, checksum.Length);

            ushort crc = CalculateCrc16(checksum);
            data[12] = (byte)(crc & 0xff);
            data[13] = (byte)(crc >> 8);

            if (showLog) LogDataAndChecksum(data, checksum, crc);

            return data;

            //byte[] response = SendCommand(inf);
            //return BitConverter.ToInt32(response, 5);
        }

        /// <summary>
        /// sync_present_position
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public byte[] SyncPresentPosition(List<byte> ids)
        {
            byte[] data = new byte[]
            {
            0xFF, 0xFF, 0xFD, 0x00, 0xFE, 0x00, 0x00, SYNC_READ, 132, 0, 0x04, 0x00
            };
            foreach (byte id in ids)
            {
                data = data.Concat(new byte[] { id, 0, 0, 0, 0 }).ToArray();
            }
            data[5] = (byte)((data.Length + 2 - 7) & 0xFF);
            data[6] = (byte)((data.Length + 2 - 7) >> 8);

            ushort[] checksum = new ushort[data.Length - 2];
            Array.Copy(data, checksum, checksum.Length);

            ushort crc = CalculateCrc16(checksum);
            data = data.Concat(new byte[] { (byte)(crc & 0xff), (byte)(crc >> 8) }).ToArray();

            if (showLog) LogDataAndChecksum(data, checksum, crc);

            return data;

            //byte[] response = SendCommand(inf);

            //Dictionary<byte, int> positions = new Dictionary<byte, int>();
            //int index = 5;
            //for (int i = 0; i < ids.Count; i++)
            //{
            //    positions[ids[i]] = BitConverter.ToInt32(response, index);
            //    index += 4;
            //}
            //return positions;
        }

        /// <summary>
        /// 送信バイトデータ確認用のメソッド
        /// </summary>
        /// <param name="data"></param>
        /// <param name="checksum"></param>
        /// <param name="crc"></param>
        private void LogDataAndChecksum(byte[] data, ushort[] checksum, ushort crc)
        {
            string dataText = "[";
            foreach (byte b in data) dataText += $"{b.ToString()}, ";
            dataText = dataText.Substring(0, dataText.Length - 2) + "]";

            string checksumText = "[";
            foreach (ushort b in checksum) checksumText += $"{b.ToString()}, ";
            checksumText = checksumText.Substring(0, checksumText.Length - 2) + "]";

            Debug.Log($"data: {dataText}, checksum: {checksumText}, result: {crc}");
        }

        public byte[] GetParameterFromPacket(byte[] receivedDates)
        {
            //check header and Read one data.
            //たまに「[255, 255, 253, 0, 1, 4, 0, 85, 7, 176, 140, 255, 255, 253, 0, 1, 4, 0, 85, 0, 161, 12]」のように２つデータを取得するので、一つに切り分ける処理を入れた。
            if (receivedDates[0] == 255 && receivedDates[1] == 255 && receivedDates[2] == 253 && receivedDates[3] == 0)
            {
                byte id = receivedDates[4];
                ushort length = (ushort)(receivedDates[6] << 8 | receivedDates[5]);
                byte instruction = receivedDates[7];
                byte error = receivedDates[8];
                byte[] paramater = new byte[length - 4];
                Array.Copy(receivedDates, 9, paramater, 0, length - 4);
                ushort Checksum = (ushort)(receivedDates[7 + length - 1] << 8 | receivedDates[7 + length - 2]);

                //check error
                switch (error)
                {
                    case 0:
                        break;
                    case 1:
                        Debug.LogError("パケット処理失敗");
                        break;
                    case 2:
                        Debug.LogError("未定義のインストラクションセット, RegWriteなしでAction");
                        break;
                    case 3:
                        Debug.LogError("CRC不一致");
                        break;
                    case 4:
                        Debug.LogError("データの最大・最小値外");
                        break;
                    case 5:
                        Debug.LogError("データ幅の不一致");
                        break;
                    case 6:
                        Debug.LogError("データのLimit値外");
                        break;
                    case 7:
                        Debug.LogError("読出専用・書込専用・ロック中のアドレスへのアクセス");
                        break;
                }

                //全パケットの表示
                string extractedDataText = "[";
                for (int i = 0; i < 9; i++) extractedDataText += $"{receivedDates[i].ToString()}, ";
                foreach (byte b in paramater) extractedDataText += $"{b.ToString()}, ";
                extractedDataText += $"{receivedDates[7 + length - 2].ToString()}, {receivedDates[7 + length - 1].ToString()}]";
                Debug.Log("Read One Data: " + extractedDataText);

                return paramater;
            }
            else
            {
                Debug.Log("received Dates are Not Byte[]");
                return null;
            }
        }
    }
}