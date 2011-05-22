このフォルダにあるスクリプトは Solar に読み込まれません。

* 内容物
: TLSpeed.py |
  タイムラインの時速 (tweet per hour) をステータスバーに表示するサンプルスクリプトです。
: PostSpeed.py |
  自分の投稿の時速 (post per hour) をステータスバーに表示するサンプルスクリプトです。
: bit.ly.py |
  bit.ly での URL 短縮を可能にするサンプルスクリプトです。

サンプルスクリプトは追加の編集による設定が必要な場合もあるので、
試用する前に必ず中身をご確認ください。

----

* スクリプトについて
IronPython 2.7 Beta 1 によるスクリプトが書けます。UTF で。

----

* スクリプトフォルダについて
- Scripts/ 直下にあるスクリプトは Solar 起動時に読み込まれます。
- 以下にあるスクリプトは Solar 起動時、およびファイルの更新時に動的に読み込まれます。
  - UrlExpander/
  - UrlShortener/
  - FilterSource/
  - FilterTerms/
- Console/ にあるスクリプトはコンソール起動時、およびファイルの更新時に動的に読み込まれます。
- 動的に読み込まれるファイルは Load(Solar.Models.Client) および Unload(Solar.Models.Client) 関数を定義しておくことで
  それぞれがスクリプトの読み込み時と解放時に実行されます。
- Scripts/ の代わりに Userscripts/ を使用すると、
  Scripts/ に同様のフォルダおよびファイル名のスクリプトが存在する場合、それぞれを比較し新しい方を読み込みます。
  Scripts/ に同様のファイル名のものが存在しない場合は、普通に読み込みます。
- フォルダ構成は起動時に読み込まれますので、Solar 起動時に該当するフォルダが無い場合は動的読み込みなフォルダも読み込まれません。
- なお Console/ 以外にあるスクリプトファイル間で名前空間は共有されません。

** UrlExpander/
短縮 URL の判定と、展開を提供します。
以下の関数を定義する必要があります:

: IsShort(uri: Uri) -> bool |
  uri に指定されたアドレスが短縮 URL であるかどうかを取得します。
: Expand(uri: Uri) -> str |
  uri に指定されたアドレスを展開します。

** UrlShortener/
長いアドレスを短縮 URL へと変換します。
以下の関数を定義する必要があります:

: Shorten(uri: Uri) -> str |
  uri に指定されたアドレスの短縮 URL を取得します。

** FilterSource/
スクリプトフィルタソースを定義します。
以下の関数を定義する必要があります:

: GetStatuses(client: Lunar.TwitterClient, range: Lunar.StatusRange) -> IEnumerable[Lunar.IEntry] |
  range で指定された範囲の項目を取得します。
  range は、一覧スクロールで取得する際に一覧の一番最後の項目が MaxID プロパティに、スクロール取得した回数が Page プロパティにセットされて渡されます。

以下の関数を定義できます:

: StreamEntryMatches(entry: Lunar.IEntry) -> bool |
  User Streams で受信した entry がこのソースで取得できるものと同等であるかを取得します。

以下の変数を定義できます:

: Pagable: bool |
  ページごとに取得し、スクロールで次のページが取得できるかどうかを True または False。
  定義されていない場合、True と判断されます。

以下の変数を使用できます:

: LocalData: ExpandoObject |
  任意のデータを保存できます。設定ファイルに記録されます。

** FilterTerms/
スクリプトフィルタ項目を定義します。
以下の関数を定義できます:

: FilterStatuses(entries: IEnumerable[Lunar.IEntry]) -> IEnumerable[Lunar.IEntry] |
  entries から任意の項目をフィルタします。定義しない場合、後述の FilterStatus を使用しフィルタリングを実行します。
: FilterStatus(entry: Lunar.IEntry) -> bool |
  entry がこのフィルタ項目に一致するかどうかを取得します。

以下の変数を使用できます:

: LocalData: ExpandoObject |
  任意のデータを保存できます。設定ファイルに記録されます。

** Console/
コンソール起動時にコンソールのコンテキストで実行されます。
以下の変数を使用できます:

: console: Solar.ConsoleWindow |
  コンソールウィンドウのインスタンスを取得します。

----

* コンソールについて
IronPython スクリプトがそのまま入力、実行できるものです。[ツール] メニューから実行できます。

help(obj) や dir(obj) を使用することにより obj にある主なメンバを確認できます。
(例えば、from Solar.Models import * した後 help(Client) すると、Client にあるメソッドを確認できます。)

** 主なキーバインド
: F1 |
  選択している領域を help() で囲みます。すでに囲まれている場合は、囲みを解除します。
: F2 |
  選択している領域を dir() で囲みます。すでに囲まれている場合は、囲みを解除します。
: ↑ |
  入力履歴を前に辿ります。
  初期設定で入力履歴に推奨される import 文があるので、開いた後↑キーを押すことにより良く使う import の入力の手間を省くことができます。
: ↓ |
  入力履歴を次に辿ります。

----

* ライブラリパスについて
IronPython の仕様上、Solar と同じフォルダに DLLs または Lib というフォルダを作ると、
そのフォルダの中のスクリプトやアセンブリを import や clr.AddReferenceToFile でロードできるようになります。

----

* 主に使いそうな要素
: Solar.Models.Client.Instance |
  Solar 本体機能です。各種イベントをハンドルすることにより色々な動作に反応できるかもしれません。
: Solar.App.Current.MainWindow |
  メインウィンドウです。MainMenu や PostTextBox, StatusBar などを使用し UI をつつけます。

----

* API 更新履歴
様々な要素は予告なく変わる可能性があります。ご注意ください。

: v1.99	|
  - Solar.Models.Client.IgnoreStreamConnection プロパティ: 追加されました。User Streams 接続要求を無視するアカウントの一覧を取得します。この HashSet に任意の AccountToken を追加することにより、そのアカウントの User Streams への接続をしないようにすることができます。
: v1.97	|
  - Solar.MainWindow.PostAccount プロパティ: 追加されました。現在の投稿先アカウントを取得または設定します。認証されているアカウント一覧に無いアカウントを設定しようとすると、例外が発生します。
: v1.92	|
  - Solar.Models.Client.StreamInitialize イベント: 追加されました。User Streams 初期化時に発生します。e.Value: TwitterStream (対象の Stream)
  - Solar.EventArgs<T1, T2> クラス: Ignition.EventArgs<T1, T2> へ移動されました。
  - Solar.ConsoleWindow クラス: コンストラクタが public になりました。多数のプロパティが追加されました。
  - Lunar.StatusCache クラス: 多数のイベントが追加されました。
: v1.89	|
  - Solar.Models.Client.Posted イベント: EventHandler<EventArgs<bool>> から EventHandler<EventArgs<AccountToken, bool>> になりました。