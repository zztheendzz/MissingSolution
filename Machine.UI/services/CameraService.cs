using MvCamCtrl.NET;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace Machine.UI.services
{
    public class CameraService
    {
        MyCamera camera = new MyCamera();
        MyCamera.MV_CC_DEVICE_INFO_LIST deviceList = new MyCamera.MV_CC_DEVICE_INFO_LIST();

        bool isGrabbing = false;
        Thread grabThread;

        public bool InitCamera()
        {
            int ret = MyCamera.MV_CC_EnumDevices_NET(
                MyCamera.MV_GIGE_DEVICE | MyCamera.MV_USB_DEVICE,
                ref deviceList);

            if (ret != 0 || deviceList.nDeviceNum == 0)
            {
                MessageBox.Show("camera = 0");
                return false;
            }    


            var deviceInfo = (MyCamera.MV_CC_DEVICE_INFO)
                Marshal.PtrToStructure(deviceList.pDeviceInfo[0],
                typeof(MyCamera.MV_CC_DEVICE_INFO));

            ret = camera.MV_CC_CreateDevice_NET(ref deviceInfo);
            if (ret != 0) return false;

            ret = camera.MV_CC_OpenDevice_NET();
            return ret == 0;
        }

        public void Start(PictureBox pictureBox)
        {
            camera.MV_CC_StartGrabbing_NET();

            isGrabbing = true;

            grabThread = new Thread(() =>
            {
                MyCamera.MV_FRAME_OUT frame = new MyCamera.MV_FRAME_OUT();

                while (isGrabbing)
                {
                    int ret = camera.MV_CC_GetImageBuffer_NET(ref frame, 1000);

                    if (ret == 0)
                    {
                        Bitmap bmp = ConvertToBitmap(frame);

                        // update UI
                        pictureBox.Invoke(new Action(() =>
                        {
                            pictureBox.Image?.Dispose();
                            pictureBox.Image = bmp;
                        }));

                        camera.MV_CC_FreeImageBuffer_NET(ref frame);
                    }
                }
            });

            grabThread.IsBackground = true;
            grabThread.Start();
        }

        public void Stop()
        {
            isGrabbing = false;
            grabThread?.Join();

            camera.MV_CC_StopGrabbing_NET();
            camera.MV_CC_CloseDevice_NET();
            camera.MV_CC_DestroyDevice_NET();
        }

        // 🔥 Convert raw → Bitmap
        private Bitmap ConvertToBitmap(MyCamera.MV_FRAME_OUT frame)
        {
            int width = frame.stFrameInfo.nWidth;
            int height = frame.stFrameInfo.nHeight;

            //  camera trả về Mono8 (grayscale)
            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format8bppIndexed);

            BitmapData bmpData = bmp.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly,
                bmp.PixelFormat);

            int stride = bmpData.Stride;
            IntPtr dst = bmpData.Scan0;

            // copy dữ liệu
            for (int i = 0; i < height; i++)
            {
                IntPtr src = frame.pBufAddr + i * width;
                IntPtr dest = dst + i * stride;
                byte[] row = new byte[width];
                Marshal.Copy(src, row, 0, width);
                Marshal.Copy(row, 0, dest, width);
            }

            bmp.UnlockBits(bmpData);

            // set grayscale palette
            ColorPalette palette = bmp.Palette;
            for (int i = 0; i < 256; i++)
            {
                palette.Entries[i] = Color.FromArgb(i, i, i);
            }
            bmp.Palette = palette;

            return bmp;
        }
    }
}
