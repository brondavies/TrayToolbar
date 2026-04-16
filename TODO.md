# TODO

## Open GitHub feature requests

- [ ] Add optional launch logging with a simple file format and configurable log path/name; capture item name, target path, timestamp, and username. (#85)
- [ ] Add a clickable link to the project's GitHub releases page from Settings or a new About dialog. (#84)
- [ ] Expand drag-and-drop support:
	- [ ] Allow dragging files and folders out of TrayToolbar menus into Explorer and other app windows. (#79, #8)
	- [ ] Allow dropping files, folders, shortcuts, and URLs into TrayToolbar so toolbars can be updated more easily. (#60)
- [ ] Support shell/system folder targets by CLSID or equivalent shell identifiers so menus can point to This PC, Network, Control Panel, Libraries, Recycle Bin, OneDrive, and user-profile locations. (#40, part of #8)
- [ ] Improve classic cascading-toolbar parity:
	- [ ] Ensure right-click consistently opens the shell context menu or Properties for menu items. (#8, follow-up to #14)
	- [ ] Support hover-only cascading navigation deeper into folders. (#8)
	- [ ] Support browsing shell items and archives where feasible. (#8)
	- [ ] Support copy/cut/paste/delete/rename workflows from the menu tree. (#8)
	- [ ] Support opening folders on double-click when appropriate. (#8)
- [ ] Add sorting and ordering options:
	- [ ] Optional strict alphabetical sort without forcing folders before files. (#42)
	- [ ] Manual custom ordering for menu items that can override automatic sorting. (#69)
- [ ] Refresh icons lazily or retry icon resolution later when shortcut targets were unavailable at startup. (#52)
- [ ] Add popup appearance options for rounded corners, darker backgrounds, and rounded hover highlights. (#70)
- [ ] Investigate and, if technically possible on stock Windows 11, support hosting the toolbar directly in the taskbar near Start. (#72)