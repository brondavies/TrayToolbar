using System.Drawing;

using TrayToolbar.Controls;

namespace TrayToolbar.Tests;

[TestClass]
public class SpinnerIconRendererTests
{
    [TestMethod]
    public void RenderFrame_returns_an_icon_of_the_same_size_for_every_frame()
    {
        using var baseIcon = (Icon)SystemIcons.Application.Clone();

        for (var frame = 0; frame < SpinnerIconRenderer.FrameCount; frame++)
        {
            using var rendered = SpinnerIconRenderer.RenderFrame(baseIcon, frame);
            Assert.AreEqual(baseIcon.Size, rendered.Size);
        }
    }
}
