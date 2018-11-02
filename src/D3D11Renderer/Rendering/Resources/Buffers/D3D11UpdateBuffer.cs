using SharpDX;
using SharpDX.Direct3D11;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.D3D11Renderer.Rendering.Resources.Buffers
{

    /// <summary>
    /// Manages an updateable D3D11 resource buffer.
    /// </summary>
    /// 
    /// Under the hood, this class manages a dynamic buffer. A local managed buffer
    /// can be updated at any time; this buffer will be copied to the dynamic buffer
    /// when the Update() method is called.
    public sealed class D3D11UpdateBuffer<T> : System.IDisposable
        where T : unmanaged
    {

        /// <summary>
        /// D3D11 device.
        /// </summary>
        private readonly D3D11Device device;

        /// <summary>
        /// GPU-side buffer.
        /// </summary>
        private Buffer gpuBuffer;

        /// <summary>
        /// Number of T objects in the buffer.
        /// </summary>
        public int Length { get; private set; }

        /// <summary>
        /// Local buffer for update.
        /// </summary>
        public T[] Buffer { get; private set; }

        /// <summary>
        /// GC handle for the local buffer.
        /// </summary>
        private GCHandle bufferHandle;

        /// <summary>
        /// Pointer to the beginning of the local buffer.
        /// </summary>
        private System.IntPtr bufferPtr;

        /// <summary>
        /// Length of the local buffer in bytes.
        /// </summary>
        private int bufferLenBytes;

        /// <summary>
        /// Creates a new updateable buffer.
        /// </summary>
        /// <param name="device">D3D11 device.</param>
        /// <param name="bindFlags">D3D11 bind flags.</param>
        /// <param name="sizeInT">Number of T objects in the buffer.</param>
        public D3D11UpdateBuffer(D3D11Device device, BindFlags bindFlags, int sizeInT)
        {
            this.device = device;
            Length = sizeInT;
            CreateBuffers(bindFlags);
            AllocateLocalMemory();
        }

        /// <summary>
        /// Updates the buffer by copying the staging buffer to the GPU buffer.
        /// </summary>
        public void Update()
        {
            var context = device.Device.ImmediateContext;

            /* Start by writing the local buffer to the buffer. */
            context.MapSubresource(gpuBuffer, MapMode.WriteDiscard, MapFlags.None, out var stream);
            stream.Write(bufferPtr, 0, bufferLenBytes);

            /* Commit the staging buffer. */
            context.UnmapSubresource(gpuBuffer, 0);
        }

        public void Dispose()
        {
            bufferHandle.Free();
        }

        /// <summary>
        /// Creates the D3D11 buffers.
        /// </summary>
        /// <param name="bindFlags">D3D11 bind flags.</param>
        private void CreateBuffers(BindFlags bindFlags)
        {
            gpuBuffer = CreateSingleBuffer(
                bindFlags,
                CpuAccessFlags.Write,
                ResourceUsage.Dynamic);
        }

        /// <summary>
        /// Allocates the local update buffer.
        /// </summary>
        private void AllocateLocalMemory()
        {
            Buffer = new T[Length];
            bufferHandle = GCHandle.Alloc(Buffer, GCHandleType.Pinned);
            bufferPtr = bufferHandle.AddrOfPinnedObject();
            bufferLenBytes = Length * Marshal.SizeOf<T>();
        }

        /// <summary>
        /// Creates a single D3D11 buffer.
        /// </summary>
        /// <param name="bindFlags">D3D11 binding flags.</param>
        /// <param name="cpuAccessFlags">D3D11 CPU access flags.</param>
        /// <param name="resourceUsage">D3D11 resource usage.</param>
        /// <returns>D3D11 buffer.</returns>
        private Buffer CreateSingleBuffer(BindFlags bindFlags, 
            CpuAccessFlags cpuAccessFlags, ResourceUsage resourceUsage)
        {
            var desc = new BufferDescription()
            {
                BindFlags = bindFlags,
                CpuAccessFlags = cpuAccessFlags,
                OptionFlags = ResourceOptionFlags.None,
                SizeInBytes = Length * Marshal.SizeOf<T>(),
                StructureByteStride = 0,
                Usage = resourceUsage
            };

            /* Create buffer. */
            return new Buffer(device.Device, desc);
        }

    }

}
