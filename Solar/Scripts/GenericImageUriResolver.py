# simple thumbnail preview
from System import *
from Solar import *

# Yfrog
ImageUriResolver.Resolvers.Add(lambda uri: ThumbnailedUri(uri, Uri(uri.AbsoluteUri + ":small")) if uri.Host == "yfrog.com" and uri.Segments.Length == 2 else None)

# Mobypicture
ImageUriResolver.Resolvers.Add(lambda uri: ThumbnailedUri(uri, Uri(uri.AbsoluteUri + ":small")) if uri.Host == "moby.to" and uri.Segments.Length == 2 else None)

# Plixi
ImageUriResolver.Resolvers.Add(lambda uri: ThumbnailedUri(uri, Uri("http://api.plixi.com/api/TPAPI.svc/imagefromurl?size=thumbnail&url=" + uri.AbsoluteUri)) if uri.Host == "plixi.com" and uri.Segments.Length == 3 and uri.Segments[-2] == "p/" else None)
ImageUriResolver.Resolvers.Add(lambda uri: ThumbnailedUri(uri, Uri("http://api.plixi.com/api/TPAPI.svc/imagefromurl?size=thumbnail&url=" + uri.AbsoluteUri)) if uri.Host == "tweetphoto.com" and uri.Segments.Length == 2 else None)

# 携帯百景
ImageUriResolver.Resolvers.Add(lambda uri: ThumbnailedUri(uri, Uri("http://image.movapic.com/pic/s_" + uri.Segments[-1] + ".jpeg")) if uri.Host == "movapic.com" and uri.Segments.Length == 3 else None)

# img.ly
ImageUriResolver.Resolvers.Add(lambda uri: ThumbnailedUri(uri, Uri("http://img.ly/show/thumb/" + uri.Segments[-1])) if uri.Host == "img.ly" and uri.Segments.Length == 2 else None)

# Twitgoo
ImageUriResolver.Resolvers.Add(lambda uri: ThumbnailedUri(uri, Uri(uri.AbsoluteUri + "/thumb")) if uri.Host == "twitgoo.com" and uri.Segments.Length == 2 else None)

# はてなフォトライフ
ImageUriResolver.Resolvers.Add(lambda uri: ThumbnailedUri(uri, Uri("http://img.f.hatena.ne.jp/images/fotolife/" + uri.Segments[1][0] + "/" + uri.Segments[1] + uri.Segments[2][ : 8] + "/" + uri.Segments[2] + "_120.jpg")) if uri.Host == "f.hatena.ne.jp" and uri.Segments.Length == 3 else None)

# Ow.ly
ImageUriResolver.Resolvers.Add(lambda uri: ThumbnailedUri(uri, Uri("http://static.ow.ly/photos/thumb/" + uri.Segments[-1] + ".jpg")) if uri.Host == "ow.ly" and uri.Segments.Length == 3 and uri.Segments[1] == "i/" else None)

# YouTube
ImageUriResolver.Resolvers.Add(lambda uri: ThumbnailedUri(uri, Uri("http://i.ytimg.com/vi/" + uri.Query[len("?v=") : ].split("&")[0] + "/default.jpg")) if uri.Host == "www.youtube.com" and uri.Segments.Length == 2 and uri.Segments[-1] == "watch" and uri.Query.StartsWith("?v=") else None)
ImageUriResolver.Resolvers.Add(lambda uri: ThumbnailedUri(uri, Uri("http://i.ytimg.com/vi/" + uri.Segments[-1] + "/default.jpg")) if uri.Host == "youtu.be" and uri.Segments.Length == 2 else None)

# pckles
ImageUriResolver.Resolvers.Add(lambda uri: ThumbnailedUri(uri, Uri(uri.AbsoluteUri + ".resize.jpg")) if uri.Host in ["pckles.com", "pckl.es"] and uri.Segments.Length == 3 else None)

# ついっぷるフォト (jpg only)
ImageUriResolver.Resolvers.Add(lambda uri: ThumbnailedUri(uri, Uri("http://p.twipple.jp/data/" + "/".join(uri.Segments[-1]) + "_s.jpg")) if uri.Host == "p.twipple.jp" and uri.Segments.Length == 2 else None)