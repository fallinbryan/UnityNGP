using System;
using System.Runtime.InteropServices;

public class NefpluginWrapper 
{
  [DllImport("unityngpp.dll", CallingConvention = CallingConvention.StdCall)]
  public static extern IntPtr GetRenderEventFunc();

  [DllImport("unityngpp.dll", CallingConvention = CallingConvention.StdCall)]
  public static extern void destroy_texture(IntPtr textureHandle);

  [DllImport("unityngpp.dll", CallingConvention = CallingConvention.StdCall)]
  public static extern System.IntPtr get_color_buffer_handle();

  [DllImport("unityngpp.dll", CallingConvention = CallingConvention.StdCall)]
  public static extern System.IntPtr get_depth_buffer_handle();

  [DllImport("unityngpp.dll", CallingConvention = CallingConvention.StdCall)]
  public static extern bool is_graphics_initialized();

  [DllImport("unityngpp.dll", CallingConvention = CallingConvention.StdCall)]
  public static extern void setup_initialization_params(string snapshot, bool use_dlss, int width, int height);

  [DllImport("unityngpp.dll", CallingConvention = CallingConvention.StdCall)]
  public static extern unsafe void udpate_veiw_matrix(float* viewMatrix);

  [DllImport("unityngpp.dll", CallingConvention = CallingConvention.StdCall)]
  public static extern unsafe void setCropBox(float* mat4x3);

  [DllImport("unityngpp.dll", CallingConvention = CallingConvention.StdCall)]
  public static extern bool isTextureCreated();

  public static void UpdateViewMatrix(float[] viewMatrix)
  {
    unsafe
    {
      fixed (float* p = viewMatrix)
      {
        udpate_veiw_matrix(p);
      }
    }
  }

  public static void SetCropBox(float scale, float[] position)
  {
    // Assuming position is a float array of length 3 (x, y, z)
    if (position.Length != 3)
    {
      throw new ArgumentException("Position must be an array of length 3.");
    }

    // Create a 4x3 matrix represented as a single array.
    // This matrix will be row-major: first three rows for rotation and scale, last row for translation.
    float[] matrix4x3 = new float[12];

    // Scale the identity matrix for rotation part
    // Assuming uniform scale for simplicity
    matrix4x3[0] = scale; // Scale X
    matrix4x3[1] = 0;
    matrix4x3[2] = 0;

    matrix4x3[3] = 0;
    matrix4x3[4] = scale; // Scale Y
    matrix4x3[5] = 0;

    matrix4x3[6] = 0;
    matrix4x3[7] = 0;
    matrix4x3[8] = scale; // Scale Z

    // Translation part
    matrix4x3[9] = position[0];
    matrix4x3[10] = position[1];
    matrix4x3[11] = position[2];


    unsafe
    {
      fixed (float* p = matrix4x3)
      {
        setCropBox(p);
      }
    }
  }
}
