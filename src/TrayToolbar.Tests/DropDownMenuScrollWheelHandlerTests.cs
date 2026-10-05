using TrayToolbar.Extensions;

namespace TrayToolbar.Tests;

[TestClass]
public class DropDownMenuScrollWheelHandlerTests
{
    const int Notch = 120;
    const int LineHeight = 24;
    const int PageHeight = 600;

    [TestMethod]
    public void GetWheelScrollDelta_scrolls_three_items_per_notch_by_default()
    {
        Assert.AreEqual(3 * LineHeight, DropDownMenuScrollWheelHandler.GetWheelScrollDelta(-Notch, 3, LineHeight, PageHeight));
        Assert.AreEqual(-3 * LineHeight, DropDownMenuScrollWheelHandler.GetWheelScrollDelta(Notch, 3, LineHeight, PageHeight));
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(5)]
    [DataRow(10)]
    public void GetWheelScrollDelta_follows_the_windows_lines_to_scroll_setting(int scrollLines)
    {
        Assert.AreEqual(scrollLines * LineHeight, DropDownMenuScrollWheelHandler.GetWheelScrollDelta(-Notch, scrollLines, LineHeight, PageHeight));
    }

    [TestMethod]
    public void GetWheelScrollDelta_scrolls_a_page_per_notch_for_one_screen_at_a_time()
    {
        Assert.AreEqual(PageHeight, DropDownMenuScrollWheelHandler.GetWheelScrollDelta(-Notch, -1, LineHeight, PageHeight));
        Assert.AreEqual(-PageHeight, DropDownMenuScrollWheelHandler.GetWheelScrollDelta(Notch, -1, LineHeight, PageHeight));
    }

    [TestMethod]
    public void GetWheelScrollDelta_scales_partial_notches_from_high_resolution_wheels()
    {
        Assert.AreEqual(3 * LineHeight / 4, DropDownMenuScrollWheelHandler.GetWheelScrollDelta(-Notch / 4, 3, LineHeight, PageHeight));
    }

    [TestMethod]
    public void GetWheelScrollDelta_scales_multiple_notches()
    {
        Assert.AreEqual(2 * 3 * LineHeight, DropDownMenuScrollWheelHandler.GetWheelScrollDelta(-2 * Notch, 3, LineHeight, PageHeight));
    }

    [TestMethod]
    public void GetWheelScrollDelta_does_not_scroll_when_scrolling_is_turned_off()
    {
        Assert.AreEqual(0, DropDownMenuScrollWheelHandler.GetWheelScrollDelta(-Notch, 0, LineHeight, PageHeight));
    }

    [TestMethod]
    public void ClampScrollDelta_passes_through_scrolls_within_range()
    {
        Assert.AreEqual(72, DropDownMenuScrollWheelHandler.ClampScrollDelta(72, firstItemTop: -100, lastItemBottom: 2000, viewTop: 20, viewBottom: 580));
        Assert.AreEqual(-72, DropDownMenuScrollWheelHandler.ClampScrollDelta(-72, firstItemTop: -100, lastItemBottom: 2000, viewTop: 20, viewBottom: 580));
    }

    [TestMethod]
    public void ClampScrollDelta_stops_at_the_first_item()
    {
        Assert.AreEqual(-30, DropDownMenuScrollWheelHandler.ClampScrollDelta(-72, firstItemTop: -10, lastItemBottom: 2000, viewTop: 20, viewBottom: 580));
        Assert.AreEqual(0, DropDownMenuScrollWheelHandler.ClampScrollDelta(-72, firstItemTop: 20, lastItemBottom: 2000, viewTop: 20, viewBottom: 580));
    }

    [TestMethod]
    public void ClampScrollDelta_stops_at_the_last_item()
    {
        Assert.AreEqual(20, DropDownMenuScrollWheelHandler.ClampScrollDelta(72, firstItemTop: -1000, lastItemBottom: 600, viewTop: 20, viewBottom: 580));
        Assert.AreEqual(0, DropDownMenuScrollWheelHandler.ClampScrollDelta(72, firstItemTop: -1000, lastItemBottom: 580, viewTop: 20, viewBottom: 580));
    }

    [TestMethod]
    public void ClampScrollDelta_does_not_scroll_when_every_item_fits()
    {
        Assert.AreEqual(0, DropDownMenuScrollWheelHandler.ClampScrollDelta(72, firstItemTop: 2, lastItemBottom: 300, viewTop: 2, viewBottom: 580));
        Assert.AreEqual(0, DropDownMenuScrollWheelHandler.ClampScrollDelta(-72, firstItemTop: 2, lastItemBottom: 300, viewTop: 2, viewBottom: 580));
    }

    [TestMethod]
    public void WinForms_scroll_button_refresh_is_still_available()
    {
        Assert.IsNotNull(DropDownMenuScrollWheelHandler.UpdateScrollButtonStatus);
    }
}
