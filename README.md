# DSGraphEdit: A reasonable facsimile of GraphEdit in .Net
## A library for adding DirectShow GraphEdit-like abilities to .Net applications.

	
Orignally published at https://www.codeproject.com/Articles/21105/DSGraphEdit-A-Reasonable-Facsimile-of-Microsoft-s

DaggerLib and DSGraphEdit is released uner the MIT license.
DirectShowLib and Media Foundation .Net are released under Lesser General Public License.

History:

	Version 1.0 - 31 Oct 2007 - Initial version
	
	Version 1.1 - 14 Dec 2007
		*	DSGraphEdit Demo now uses the Weifenluo DockPanel Suite for MDI style multiple graph editing.
		*	Major optimizations to DaggerLib.UI.Windows rendering speed.
		*	Full source to DaggerLib.Core and DaggerLib.UI.Windows now included in Solution.
		*	Added GraphNavigator control for easier editing/browsing of large graphs.
		*	Pin names are drawn inside nodes when not viewing non-modal properties.
		*	Arrange Nodes now accounts for pin names that are drawn on the Canvas.
		*	InitVideoWindow in VideoInternalWindow would fail when loading a Graph that had an unconnected video renderer.
		*	VideoInternalWindow now supports loading graphs that contain an EVR filter.
		*	SyncGraph now removes/re-routes connections.
		*	VideoInternalWindow posts mouse location and mouse clicks to attached DVD Navigators for interaction with DVD menus.
		*	Filter Nodes hosting a VideoInternalWindow connected to DVD Navigators now have a DVD Chapters button.
		*	VideoInternalWindow now supports windowed full screen.
		*	Added CanvasImage property to DSGraphEditPanel for retrieving a snapshot of the Canvas.
		*	Now scans ALL interfaces Pins and Filters support (not just the DirectShow interfaces).
		*	Fixed some property pages not being visible in non-modal mode.

	Version 1.2 - 20 Jan 2018
    	*	Fixed issue with DaggerUIGraph.KeyboardProc on 64-bit systems
    	*	Added 64/32 bit runtime indiaction on main form.
    	*	Added project for "run as 32"
    	*	License changed to MIT
		*	Updated MediaFoundationNet version
		*	Updated DirectShowLib version