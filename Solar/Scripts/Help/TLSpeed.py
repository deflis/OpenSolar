# Solar 1.84 Timeline speed indicator
"""
概要:
	タイムラインの時速を表示します。

導入方法:
	Scripts/ または Userscripts/ に本ファイルをコピーします。

使用方法:
	Solar 起動時に自動的にステータスバーに新しいパネルが追加されます。
	例えば est.100t/h と表示されていれば、
	est. は起動後一時間経っておらず数字が不正確なことを表します。
	100t/h は今から一時間前までの取得件数が 100 であることを表します。
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
	p.Content = "0t/h"
	Client.Instance.Refreshed += Update
	Client.Instance.RequestedNewPage += Update
	DockPanel.SetDock(p, Dock.Right)
	f.StatusBar.Items.Insert(f.StatusBar.Items.Count - 1, p)

def Update(sender, e):
	spd = str(len([i for i in Client.Instance.StatusCache.GetStatuses() if i != None and i.CreatedAt >= DateTime.Now - TimeSpan.FromHours(1)]))
	
	if (DateTime.Now - startup).TotalHours < 1:
		spd = "est." + spd
	
	p.Content = spd + "t/h"

App.Current.Dispatcher.Invoke(Action(Load))