# bit.ly url shortener
"""
概要:
	bit.ly での URL 短縮を可能にします。

導入方法:
	以下の login および apiKey を適切な値に設定した後、
	Scripts/UrlShortener または Userscripts/UrlShortener に本ファイルをコピーします。

使用方法:
	投稿ボックスのコンテキストメニューの URL 短縮メニューから選択し、投稿ボックスの URL を短縮します。
"""

# ユーザ名 を設定してください
login = ""

# API キー を設定してください
apiKey = ""

from System import *
from System.Net import *

def Shorten(uri = Uri):
	with WebClient() as wc:
		return wc.DownloadString(str.Format("http://api.bit.ly/v3/shorten?format=txt&login={0}&apiKey={1}&longUrl={2}",
											login,
											apiKey,
											Uri.EscapeDataString(uri.AbsoluteUri)))