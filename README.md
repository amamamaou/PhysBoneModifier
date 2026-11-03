# Amama PhysBone Modifier

VRChat アバターの PhysBone 設定を NDMF のビルド時に非破壊で変更します。

## 動作環境

- Unity 2022.3
- VRChat SDK - Avatars（package.json の依存指定に従います）
- NDMF 1.14.8 以降（現在の開発環境を基準とした下限です。旧バージョンは未検証）

## 使い方

1. アバタープロジェクトに VRChat SDK - Avatars と NDMF を導入します。
2. 本パッケージを `Packages/com.amama.physbone-modifier` に配置します。
3. アバターまたはその子に `Amama/PhysBone Modifier` を追加します。
4. Allow Collision / Allow Grabbing / Allow Posing を設定します。「継承／変更なし」は親の設定を引き継ぎ、親からの設定がない項目は PhysBone の元の値を保持します。Other は Self / Others を個別に指定します。
5. 除外するオブジェクトを指定します。その子も除外されます。

コンポーネントを置いたオブジェクト以下の PhysBone が対象です（非アクティブも含む）。NDMF の Resolving フェーズでビルド用コピーに適用し、元のアバターは変更しません。
他ツールが後続の Generating フェーズなどで生成する PhysBone の設定は、そのツールに任せます。この処理より前に生成された PhysBone は既存のものと区別できないため、対象に含まれます。
親子にコンポーネントを配置した場合は、親から子の順に適用し、各項目で子が明示した設定を優先します。子の「継承／変更なし」は、親の設定を適用した後の値を保持します。
例えば、親の Allow Grabbing が False、子が True なら、子以下では True になります。子が「継承／変更なし」なら、親の False を引き継ぎます。
無効化したコンポーネントは適用しません。除外指定は、そのコンポーネントの処理だけに適用され、子のコンポーネントの処理を禁止しません。
同じオブジェクトに複数配置した場合は、コンポーネントの取得順で後の設定が優先されます。優先順位を明確にするには、親子に分けて配置してください。

## Avatar Optimizer との互換性

設定適用後、Resolving フェーズ内でビルド用コピーから本コンポーネントを削除します。無効なコンポーネントも削除しますが、配置先の GameObject と元のアバターは削除しません。
これにより AAO の最適化時に未知のコンポーネントとして残りません。AAO の API 登録や必須依存は追加していません。
AAO Merge PhysBone 側で Override した項目は AAO 側の指定が優先されます。

[AAO の互換対応ガイド](https://vpm.anatawa12.com/avatar-optimizer/ja/docs/developers/make-your-components-compatible-with-aao/) のビルド時専用コンポーネント向けの推奨に従っています。Unity 上での併用ビルドは別途確認が必要です。

## ライセンス

MIT（LICENSE を参照）。
