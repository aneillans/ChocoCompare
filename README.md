# ChocoCompare
A small tool to compare a local Chocolatey feed to the main public feed for updates

For a while now I've been using a combination of Boxstarter and Chocolatey to help me manage and maintain by devices. But one of the snags that I have encountered is ensuring that my local, moderated feed is up to date with the public feed.

You are probably wondering ... why do I bother with a private package feed? Well, two reasons:

- Offline support; I often end up having to rebuild devices while away from home - and hotels don't have great wifi. I travel with a pre-loaded USB key with a copy of my feed (and installers) for just this reason

- Control; I like to keep control of what versions are on my devices and don't particularly like being forced onto the latest and greatest - and I like to ensure all my devices are on the same version ;)

So I wrote a very simple tool to compare my local package feed to the chocolatey public feed

From blog: https://www.neillans.co.uk/post/2016/09/23/chocolatey-and-keeping-your-local-feed-up-to-date
