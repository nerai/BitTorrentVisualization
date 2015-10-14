# BitTorrentVisualization
A simplified simulation of a bittorrent network

This is one of the projects I did to learn C# back in 2010. I touched it again in 2015, brushed it up a bit and put it on github for your enjoyment. For a Winforms-only application, it looks pretty decent, but do not expect high quality code behind.

This application simulates a dynamic network of nodes which communicate via the [bittorrent protocol](https://en.wikipedia.org/wiki/BitTorrent). This basically means they all try to download some data, which they share among each other in pieces. This continues until everyone has all pieces, i.e. all nodes finished the download. While this is simple enough, the matter gets more complicated by new nodes entering or leaving the network, which means not all pieces may be available.

Note that while this simulation superficially does resemble an actual network, it is not technically accurate. Many aspects were dumbed down or removed entirely. I'm not going to write a physically accurate TCP/IP simulation here.
