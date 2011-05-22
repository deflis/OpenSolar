from System import *
from System.Net import *

cache = {}

def ClearCache(sender, e):
	cache.clear()

def Load(client):
	client.ClearCache += ClearCache

def Unload(client):
	client.ClearCache -= ClearCache

def IsShort(uri = Uri):
	return uri.Host in ["tinyurl.com",
						"t.co",
						"is.gd",
						"snipurl.com",
						"snurl.com",
						"tiny.cc",
						"tiny.ly",
						"urlenco.de",
						"bit.ly",
						"pickl.es",
						"qurlyq.com",
						"nsfw.in",
						"dwarfurl.com",
						"icanhaz.com",
						"piurl.com",
						"linkbee.com",
						"traceurl.com",
						"twurl.nl",
						"cli.gs",
						"rubyurl.com",
						"nav.cx",
						"budurl.com",
						"ff.im",
						"twitthis.com",
						"blip.fm",
						"goo.gl",
						"tumblr.com",
						"ustre.am",
						"qurl.com",
						"pic.gd",
						"digg.com",
						"bctiny.com",
						"5jp.net",
						"j.mp",
						"ow.ly",
						"bkite.com",
						"youtu.be",
						"divr.it",
						"mixi.bz",
						"p.tl",
						"nico.ms",
						"ht.ly",
						"2ch2.net",
						"tl.gd",
						"htn.to",
						"amzn.to",
						"flic.kr",
						"moi.st",
						"ux.nu"]

def Expand(uri = Uri):
	if uri in cache:
		return cache[uri]
	else:
		try:
			req = HttpWebRequest.Create(uri)
			req.AllowAutoRedirect = False
			req.Timeout = 5000
			req.Method = "HEAD";
			cache[uri] = req.GetResponse().Headers[HttpResponseHeader.Location]

			return cache[uri]
		except Exception, ex:
			return None