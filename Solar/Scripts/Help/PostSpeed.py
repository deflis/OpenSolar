# Solar 1.88 Post speed indicator
"""
概要:
	自分の投稿時速を表示します。

導入方法:
	Scripts/ または Userscripts/ に本ファイルをコピーします。

使用方法:
	Solar 起動時に自動的にステータスバーに新しいパネルが追加されます。
	例えば est.10p/h と表示されていれば、
	est. は起動後一時間経っておらず数字が不正確なことを表します。
	10p/h は今から一時間前までの自分の投稿件数が 10 であることを表します。
"""
from System import *
from System.Windows.Controls import *
from System.Windows.Controls.Primitives import *
from Solar import *
from Solar.Models import *

p = None
startup = DateTime.Now

def Load():
	global p
	
	f = App.Current.MainWindow
	p = StatusBarItem()
	p.Content = "0p/h"
	Client.Instance.Refreshed += Update
	Client.Instance.RequestedNewPage += Update
	DockPanel.SetDock(p, Dock.Right)
	f.StatusBar.Items.Insert(f.StatusBar.Items.Count - 1, p)

def Update(sender, e):
	items = [i for i in Client.Instance.StatusCache.GetStatuses() if i != None and i.CreatedAt >= DateTime.Now - TimeSpan.FromHours(1)]
	accounts = [i.Name + ": " + ("est." if (DateTime.Now - startup).TotalHours < 1 else "") + str(len([j for j in items if j.UserName == i.Name])) + "p/h" for i in Client.Instance.Accounts]
	p.ToolTip = "\r\n".join(accounts)
	
	text = [i for i in accounts if i != "est.0p/h" and i != "0p/h"]
	p.Content = text[0].split(": ", 2)[-1] if any(text) else accounts[0].split(": ", 2)[-1]

App.Current.Dispatcher.Invoke(Action(Load))