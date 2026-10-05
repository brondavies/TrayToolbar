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
    public void GetScrollIntoViewDelta_leaves_a_visible_item_alone()
    {
        Assert.AreEqual(0, DropDownMenuScrollWheelHandler.GetScrollIntoViewDelta(itemTop: 20, itemBottom: 44, viewTop: 20, viewBottom: 580));
        Assert.AreEqual(0, DropDownMenuScrollWheelHandler.GetScrollIntoViewDelta(itemTop: 556, itemBottom: 580, viewTop: 20, viewBottom: 580));
    }

    [TestMethod]
    public void GetScrollIntoViewDelta_scrolls_down_to_an_item_below_the_view()
    {
        Assert.AreEqual(500, DropDownMenuScrollWheelHandler.GetScrollIntoViewDelta(itemTop: 1056, itemBottom: 1080, viewTop: 20, viewBottom: 580));
    }

    [TestMethod]
    public void GetScrollIntoViewDelta_scrolls_up_to_an_item_above_the_view()
    {
        Assert.AreEqual(-120, DropDownMenuScrollWheelHandler.GetScrollIntoViewDelta(itemTop: -100, itemBottom: -76, viewTop: 20, viewBottom: 580));
    }

    [TestMethod]
    public void GetScrollIntoViewDelta_shows_a_partly_hidden_item_in_full()
    {
        Assert.AreEqual(10, DropDownMenuScrollWheelHandler.GetScrollIntoViewDelta(itemTop: 566, itemBottom: 590, viewTop: 20, viewBottom: 580));
        Assert.AreEqual(-10, DropDownMenuScrollWheelHandler.GetScrollIntoViewDelta(itemTop: 10, itemBottom: 34, viewTop: 20, viewBottom: 580));
    }

    [TestMethod]
    public void GetScrollIntoViewDelta_aligns_an_item_taller_than_the_view_to_the_top()
    {
        Assert.AreEqual(980, DropDownMenuScrollWheelHandler.GetScrollIntoViewDelta(itemTop: 1000, itemBottom: 1700, viewTop: 20, viewBottom: 580));
    }

    [TestMethod]
    public void WinForms_scroll_button_refresh_is_still_available()
    {
        Assert.IsNotNull(DropDownMenuScrollWheelHandler.UpdateScrollButtonStatus);
    }
}
