using ControlBeanExDll;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TcpserverExDll;

namespace FieldScan
{
    public struct Pt
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float R { get; set; }

    }
    public class ScanClass
    {
        private ControlBeanEx robot;

        public void Init()
        {
            try
            {
                int robotId = 82;
                //
                TcpserverEx.net_port_initial();
                Thread.Sleep(3000);

                robot = TcpserverEx.get_robot(robotId);
                int i = 0;
                for (i = 0; i < 10; i++)
                {
                    Thread.Sleep(500);
                    if (robot.is_connected()) break;
                }
                if (i == 10)
                {
                    throw new Exception("超时！");
                }
                //TcpserverEx.close_tcpserver();
                int state = robot.initial(1, 210);
                if (state == 1)
                {
                    robot.unlock_position();
                    //Console.WriteLine("初始化成功！");
                }
                else
                {
                    //if (state == 105)
                    //{
                    //    robot.joint_home(1);
                    //    robot.joint_home(2);
                    //    robot.joint_home(3);
                    //    robot.joint_home(4);
                    //}
                    throw new Exception("初始化错误！");

                }
                robot.get_scara_param();

                //Console.WriteLine("需要初始化？");
                //Console.WriteLine(robot.get_joint_state(1));
                //Console.WriteLine(robot.get_joint_state(2));
                //Console.WriteLine(robot.get_joint_state(3));
                //Console.WriteLine(robot.get_joint_state(4));
                //Console.WriteLine("拖动");
                //Console.WriteLine(robot.get_drag_teach());
                //Console.WriteLine("协作");
                //Console.WriteLine(robot.get_cooperation_fun_state());
                //Console.WriteLine("碰撞");
                //Console.WriteLine(robot.is_collision());

                //robot.set_catch_or_release_accuracy(0.1f);//z轴判断标准
                robot.set_allow_offset_at_target_position(0.1f, 0.1f, 0.1f, 0.1f);//x,y,z  (mm),r  (deg)

                robot.new_set_acc(50, 50, 50, 50);//百分比，30-220

                robot.set_drag_teach(false);
            }
            catch (Exception)
            {
                throw;
            }

        }

        public Pt GetPos()
        {
            robot.get_scara_param();
            return new Pt { X = robot.x, Y = robot.y, Z = robot.z, R = robot.rotation };
        }
        public bool CanGo(float x, float y, float z, float r)
        {
            return robot.judge_in_range(x, y, z, r);//x,y,z  (mm),r  (deg)//判断能否达到。
        }
        public void Go(float x, float y, float z, float r, float spd)
        {
            robot.new_movej_xyz_lr(x, y, z, r, spd, 1, y > 0 ? 1 : -1);
            //robot.new_move_xyz(x, y, z, 0, 10, y > 0 ? 1 : -1, 1);
            //Console.WriteLine("Bigin Run");
            for (int k = 0; k < 100; k++)
            {
                Thread.Sleep(500);
                if (robot.is_robot_goto_target())
                {
                    robot.get_scara_param();
                    //Console.WriteLine("到达！");
                    return;
                }
            }
            throw new Exception("超时未到达！");
        }

        // 新增一个非阻塞的移动方法，用于连续控制
        public void StartMove(float x, float y, float z, float r, float spd)
        {
            // 这条指令会立即发送移动命令，然后程序会继续往下执行，不会在此等待
            robot.new_movej_xyz_lr(x, y, z, r, spd, 1, y > 0 ? 1 : -1);
        }

        public void Close()
        {
            TcpserverEx.close_tcpserver();
        }
        //static void Write(double[] datas, int x, int y)
        //{
        //    Task.Run(() =>
        //    {
        //        var path = $"{folder}\\Data_{x}_{y}.csv";
        //        if (!Directory.Exists(path)) Directory.CreateDirectory(Path.GetDirectoryName(path));
        //        StreamWriter sw = new StreamWriter(path);
        //        for (int i = 0; i < datas.Length; i++)
        //        {
        //            sw.WriteLine(datas[i]);
        //        }
        //        sw.Close();
        //        sw.Dispose();
        //    });
        //}
    }
}
