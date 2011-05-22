# Google url shortener
from System import *
from System.Net import *

def Shorten(uri = Uri):
	with WebClient() as wc:
		wc.UploadDict("http://goo.gl/api/shorten", { "url": uri.AbsoluteUri, "security_token": "null" })
		
		return wc.ResponseHeaders[HttpResponseHeader.Location]