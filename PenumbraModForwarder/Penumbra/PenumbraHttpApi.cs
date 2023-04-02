// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Penumbra;

using System.Threading.Tasks;

public static partial class PenumbraHttpApi {
    public static async Task Redraw(int targetIndex) {
        RedrawData data = new();
        data.ObjectTableIndex = targetIndex;
        data.Type = RedrawData.RedrawType.Redraw;

        await PenumbraApi.Post("/redraw", data);

        await Task.Delay(500);
    }
    public static async Task Reload(string modPath, string modName) {
        ModReloadData data = new ModReloadData(modPath, modName);
        await PenumbraApi.Post("/reloadmod", data);

        await Task.Delay(500);
    }
    public static async Task Unpack(string modPath) {
        ModUnpackData data = new ModUnpackData(modPath);
        await PenumbraApi.Post("/unpackmod", data);

        await Task.Delay(500);
    }

    private record ModReloadData(string Path, string Name) {
        public ModReloadData()
            : this(string.Empty, string.Empty) { }
    }

    private record ModUnpackData(string Path) {
        public ModUnpackData()
            : this(string.Empty) { }
    }

    public class RedrawData {
        public enum RedrawType {
            Redraw,
            AfterGPose,
        }

        public string Name { get; set; } = string.Empty;
        public int ObjectTableIndex { get; set; } = -1;
        public RedrawType Type { get; set; } = RedrawType.Redraw;
    }
}
