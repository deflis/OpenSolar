# TinyURL url shortener
from System import *
from System.Net import *

def Shorten(uri = Uri):
	with WebClient() as wc:
		return wc.DownloadString("http://tinyurl.com/api-create.php?url=" + Uri.EscapeDataString(uri.AbsoluteUri))