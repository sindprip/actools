using AcTools.Render.Base.Cameras;
using JetBrains.Annotations;

namespace AcTools.Render.Kn5Specific {
    public interface IKn5ObjectRenderer {
        [NotNull]
        BaseCamera Camera { get; }

        [CanBeNull]
        CameraOrbit CameraOrbit { get; }

        bool AutoRotate { get; set; }

        bool AutoAdjustTarget { get; set; }

        bool VisibleUi { get; set; }

        bool CarLightsEnabled { get; set; }

        void SelectPreviousSkin();

        void SelectNextSkin();

        void SelectSkin(string skinId);

        void ResetCamera();
    }
}