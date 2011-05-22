# p.tl or pixiv thumbnail preview
import clr

clr.AddReference("System.Xml.Linq")

from System import *
from System.IO import *
from System.Net import *
from System.Xml.Linq import *
from Solar import *
from Solar.Models import *
from Ignition.Sgml import *

cache = {}

def ConvertPixiv(uri = Uri):
	if uri.Host == "p.tl" and uri.Segments.Length == 3 and uri.Segments[1] == "i/" or uri.Host == "www.pixiv.net" and uri.AbsolutePath == "/member_illust.php" and uri.Query.Contains("illust_id="):
		return ThumbnailedUri(uri, ResolvePixiv)
	else:
		return None

def ResolvePixiv(uri = Uri):
	if uri in cache:
		return cache[uri]
	
	try:
		with WebClient() as wc:
			tmp = Path.GetTempFileName()
			illustUri = "http://www.pixiv.net/member_illust.php?mode=medium&illust_id=" + uri.Segments[-1] if uri.Host == "p.tl" else uri.AbsoluteUri
			id = illustUri[illustUri.find("illust_id=") + len("illust_id=") : ]

			with SgmlReader(illustUri) as sr:
				xml = XDocument.Load(sr)
				ns = xml.Root.Name.Namespace
				ls = [j for j in [Uri(i.Attribute("src").Value) for i in xml.Descendants(ns + "img")] if j.Segments[-1].Contains(id + "_m") or j.Segments[-1].Contains(id + "_s")]
			
				if not any(ls):
					return None
			
				rt = ls[0]
				wc.Headers.Add(HttpRequestHeader.Referer, sr.Href)
		
			tmp2 = Path.Combine(Path.GetDirectoryName(tmp), Path.GetFileNameWithoutExtension(tmp) + Path.GetExtension(rt.Segments[-1]))
			wc.DownloadFile(rt, tmp)
			File.Move(tmp, tmp2)
			cache[uri] = Uri(tmp2)

			return cache[uri]
	except WebException:
		return None


def ClearPixiv(sender, e):
	for i in cache:
		File.Delete(cache[i].AbsolutePath)

	cache.clear()

ImageUriResolver.Resolvers.Add(ConvertPixiv)
Client.Instance.Shutdown += ClearPixiv
Client.Instance.ClearCache += ClearPixiv
