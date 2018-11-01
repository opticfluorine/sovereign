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
    /// Under the hood, this class actually manages two buffers - one
    /// with D3D11_USAGE_DEFAULT that is used by the GPU, and one with
    /// D3D11_USAGE_STAGING that is used to stage updates prior to use.
    public sealed class D3D11UpdateBuffer<T> where T : unmanaged
    {

        public D3D11UpdateBuffer(D3D11Device device, int len)
        {

        }

        private Buffer CreateSingleBuffer(D3D11Device device, BindFlags bindFlags, 
            CpuAccessFlags cpuAccessFlags, ResourceUsage resourceUsage, 
            int sizeInT, out GCHandle bufferHandle)
        {
            var desc = new BufferDescription()
            {
                BindFlags = bindFlags,
                CpuAccessFlags = cpuAccessFlags,
                OptionFlags = ResourceOptionFlags.None,
                SizeInBytes = sizeInT * Marshal.SizeOf<T>(),
                StructureByteStride = 0,
                Usage = resourceUsage
            };

            /* Pin the buffer storage so we can take a pointer. */
            var data = new T[sizeInT];
            bufferHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            var dataPtr = bufferHandle.AddrOfPinnedObject();

            /* Create buffer. */
            return new Buffer(device.Device, dataPtr, desc);
        }

    }

}
