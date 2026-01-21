# Attendance Record

勤務・休憩の開始/終了をワンクリックで記録し、累計時間を計算するデスクトップアプリです。

システムトレイ／メニューバー常駐で動作し、ウィンドウを閉じてもバックグラウンドでタイマーを更新します。

---

## 動作環境

### 配布バイナリ

- Windows: Windows 10/11
- macOS: 10.13+
- Linux: 各種ディストリビューション (X11 or Wayland)

※ 配布物は `dotnet publish`（AOT）によるビルドを使用しているため、.NET ランタイムのインストールは不要です。

### 開発（ソースからビルド）

- .NET SDK（`net10.0` 対応）

---

## 主な機能

- **勤務開始 / 勤務終了**（ホーム画面）
- **休憩開始 / 休憩終了**（勤務中のみ有効）
- **勤務時間・休憩時間・残業時間の集計**
  - 今日の残業 / 今月の残業（標準勤務時間の設定に基づく）
- **勤務記録をクリップボードへコピー**（書式カスタマイズ可）
- **履歴（月次）閲覧・日別編集**
  - 出退勤（開始/終了）の編集
  - 休憩 / 有給休暇（複数）を追加・編集・削除
  - 勤務記録の削除
- **アラーム**
  - 勤務終了前アラーム（スヌーズ対応）
  - 休憩開始アラーム（スヌーズ対応）
- **単一起動**（2重起動すると既存インスタンスのウィンドウを表示）
- **テーマ**（システム/ライト/ダーク）

---

## 使い方

### 1) 起動・終了

- 起動すると、トレイ（メニューバー）にアイコンが表示されます。
- ウィンドウを閉じてもアプリは終了しません（**ウィンドウは非表示**になります）。
- 終了するには、トレイメニューの **「終了」** を選択します。

### 2) ホーム

- **勤務開始 / 勤務終了**
  - 勤務時間の開始と終了を記録します。
- **休憩開始 / 休憩終了**
  - 休憩時間の開始と終了を記録します。
- **勤務記録のコピー**
  - 画面右下のコピーアイコンで、現在の集計記録をクリップボードへコピーします。(設定から書式をカスタマイズ可)

### 3) 履歴

- 左のリストから日付を選ぶと、その日の勤務記録を表示・編集できます。
- ツールバーで月期間の移動、本日の記録への移動、特定の日付への移動ができます。
- 勤務が終了しているにも関わらず、退勤が未入力、または休憩の終了が未入力の場合、リストの日付の横に警告アイコンが表示されます。

#### 編集できる内容

- **出退勤**：開始（必須）/ 終了（任意）
- **休憩・有給休暇**：複数追加可能。
  - 「休憩」：開始時間は必須。終了は任意。（終了時間が空の場合、休憩中とみなします）
  - 「有給休暇」：開始時刻・終了時刻の両方とも必須。

### 4) 設定

- **テーマ設定**：UIのテーマ設定を行います。 (システムに合わせる / ライト / ダーク )
- **標準勤務時間（分）**：一日の標準勤務時間を設定。残業計算とアラーム判定に使用します。
- **勤務終了前アラーム**：勤務終了前にアラームを表示する設定
  - 有効/無効、終了の何分前に出すか、スヌーズの分数
- **休憩開始アラーム**：休憩開始前にアラームを表示する設定
  - 有効/無効、勤務開始から何分後に出すか、スヌーズの分数
- **勤務記録コピーの書式**：テンプレートを編集できます（下記参照）
- **設定フォルダ**：アプリデータの保存先フォルダを開きます

---

## 勤務記録コピーの書式（テンプレート）

テンプレート中のプレースホルダーを置換して、クリップボードへコピーします。

利用可能なプレースホルダー：

- 勤務時間：`{work_hh}` / `{work_mm}` / `{work_ss}`
- 休憩時間：`{rest_hh}` / `{rest_mm}` / `{rest_ss}`
- 今日の残業：`{over_hh}` / `{over_mm}` / `{over_ss}`
- 今月の残業：`{over_monthly_hh}` / `{over_monthly_mm}` / `{over_monthly_ss}`

---

## データの保存先

アプリのデータは OS のアプリケーション設定フォルダ配下に保存されます。

- `appConfig.json`：アプリ設定
- `workRecords.json`：勤務記録
- `*.bak`：バックアップ
- `app.lock`：単一起動のロックファイル

### 保存先のパス

- Windows: `C:\{User}\AppData\Roaming\AttendanceRecord\`
- macOS：`~/Library/Application Support/AttendanceRecord/`
- Linux：`~/.config/AttendanceRecord/`

---

## ビルド / 実行（開発者向け）

### 起動（開発）

```bash
dotnet run --project AttendanceRecord.Presentation
```

### 発行

```bash
dotnet publish -c Release AttendanceRecord.Presentation
```

Mac用にはApple Silicon 向けの簡易バンドル生成スクリプトがあります：

```bash
./publish_mac_app.sh
```

- 成果物：`AttendanceRecord.app`
- 内部で `dotnet publish -c Release AttendanceRecord.Presentation` を実行します。

環境によっては明示的な RID 指定が必要です：

```bash
dotnet publish -c Release -r osx-arm64 AttendanceRecord.Presentation
```

---

## プロジェクト構成

- `AttendanceRecord.Presentation`：UI（Avalonia）
- `AttendanceRecord.Application`：アプリケーションサービス
- `AttendanceRecord.Domain`：ドメイン層
- `AttendanceRecord.Infrastructure`：インフラ層
- `AttendanceRecord.Persistence`：永続化 DTO / JSON Context 等（C#）
- `AttendanceRecord.Shared`：共通ユーティリティ

---

## Third Party Notices

依存関係やアセットのライセンス表記は [THIRD-PARTY-NOTICES.txt](THIRD-PARTY-NOTICES.txt) を参照してください。

---

## ライセンス

MIT License. See [LICENSE.txt](LICENSE.txt).

---

## コントリビュート

Issue / PR 歓迎です。バグ報告の際は、OS・.NET SDK バージョン・再現手順を添えてください。
