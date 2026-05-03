window.BENCHMARK_DATA = {
  "Callback": {
    "commits": [
      {
        "sha": "3eb0d03f222ccc5ac42a2f32e78546cc9c633150",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T13:26:28Z",
        "message": "feat: add baseline benchmark comparison to reporting (#744)"
      },
      {
        "sha": "381840acc2d2a841abbc64369aede5f364acdb1f",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T13:54:51Z",
        "message": "refactor: enhance benchmark table display names (#746)"
      },
      {
        "sha": "ace2d4ac2db5c68606ef27eb394c78f514e5557d",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T15:29:24Z",
        "message": "coverage: add build tests (#747)"
      },
      {
        "sha": "aac7d3511c8ac5e09b9d3ef995fc3b52c70b098a",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T15:52:53Z",
        "message": "perf: optimize generator-emitted setups by skipping dual registration (#741)"
      },
      {
        "sha": "c5047af16fa6992d7704cbfa089919c96091d884",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T16:25:14Z",
        "message": "fix: build error (#749)"
      },
      {
        "sha": "7f5a215d0dcc2c82a88f3fa2bd0aa7666848b9f8",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T16:39:28Z",
        "message": "perf: lazy-allocate per-member fast buffers on first record (#740)"
      },
      {
        "sha": "c28709e4647b9abe55894504efcf4d69941173cb",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T20:01:07Z",
        "message": "feat: bind \u0060Method(default, \u2026)\u0060 to the by-values overload (#750)"
      },
      {
        "sha": "200d0bb9827367185c99c06a7d05d7c9f84701bc",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T20:40:13Z",
        "message": "docs: update migration package description to include NSubstitute support (#751)"
      },
      {
        "sha": "64156009dafaca073bdfea9a5b04562dee1cc050",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-02T06:08:23Z",
        "message": "refactor: move source-generated mock to top of file (#752)"
      },
      {
        "sha": "681a2cfdd105fbed7fdd389922b2aee58bae897f",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-02T06:28:36Z",
        "message": "fix: make source generator snapshot build configuration invariant (#753)"
      },
      {
        "sha": "812a93f05fb7bbaf016d8b6e82a455a4270e591e",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-02T06:44:29Z",
        "message": "refactor: enable nullable reference types in BenchmarkReport (#754)"
      },
      {
        "sha": "a56e91e48833343e983992a69d2fe77b953b8f9c",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-02T20:56:04Z",
        "message": "docs: notify Testably.Site when documentation changes (#755)"
      },
      {
        "sha": "a3038970faba5546782c1657f6f0927ca5fcd930",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-02T22:10:02Z",
        "message": "refactor: update repository references from aweXpect to Testably (#756)"
      },
      {
        "sha": "557bcbc8ab11c6c1c0101656ed73d210511ba24b",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-03T05:56:25Z",
        "message": "fix: casing of sonar organization (#760)"
      }
    ],
    "labels": [
      "3eb0d03f",
      "381840ac",
      "ace2d4ac",
      "aac7d351",
      "c5047af1",
      "7f5a215d",
      "c28709e4",
      "200d0bb9",
      "64156009",
      "681a2cfd",
      "812a93f0",
      "a56e91e4",
      "a3038970",
      "557bcbc8"
    ],
    "datasets": [
      {
        "label": "Mockolate time",
        "unit": "ns",
        "data": [
          392.565998617808,
          397.3738365854536,
          364.8504521369934,
          319.0067191441854,
          242.85250539046066,
          319.0333944956462,
          377.7441614151001,
          324.8121613820394,
          308.91970415910083,
          351.44624350865683,
          311.13388564036444,
          334.6883158365885,
          390.8558732668559,
          335.1184697491782
        ],
        "borderColor": "#63A2AC",
        "backgroundColor": "#63A2AC",
        "yAxisID": "y",
        "borderDash": [],
        "pointStyle": "circle"
      },
      {
        "label": "Mockolate memory",
        "unit": "b",
        "data": [
          1832,
          1832,
          1832,
          1720,
          1720,
          1720,
          1720,
          1720,
          1720,
          1720,
          1720,
          1720,
          1720,
          1720
        ],
        "borderColor": "#63A2AC",
        "backgroundColor": "#63A2AC",
        "yAxisID": "y1",
        "borderDash": [
          5,
          5
        ],
        "pointStyle": "triangle"
      },
      {
        "label": "Moq time",
        "unit": "ns",
        "data": [
          95985.198828125,
          70481.12766676683,
          72225.42204938616,
          97235.4769193209,
          56782.53134390024,
          98309.23843819754,
          72812.66158353366,
          71665.97180175781,
          70116.81715494792,
          98867.70450265067,
          69679.67070661273,
          97096.40757533482,
          71915.64620535714,
          97602.78458658855
        ],
        "borderColor": "#A052B0",
        "backgroundColor": "#A052B0",
        "yAxisID": "y",
        "borderDash": [],
        "pointStyle": "circle"
      },
      {
        "label": "Moq memory",
        "unit": "b",
        "data": [
          9096,
          9096,
          9096,
          9096,
          9095,
          9096,
          9090,
          9096,
          9096,
          9096,
          9096,
          9096,
          9090,
          9096
        ],
        "borderColor": "#A052B0",
        "backgroundColor": "#A052B0",
        "yAxisID": "y1",
        "borderDash": [
          5,
          5
        ],
        "pointStyle": "triangle"
      },
      {
        "label": "NSubstitute time",
        "unit": "ns",
        "data": [
          4435.9180379231775,
          4396.509897359212,
          4180.564256395613,
          4412.009573872884,
          3446.4735900878904,
          4458.977124350412,
          4236.015572102865,
          4287.988632202148,
          4139.811647542318,
          4611.654821777343,
          4001.651503426688,
          4373.678816114153,
          4357.896810259138,
          4406.545623779297
        ],
        "borderColor": "#5E2750",
        "backgroundColor": "#5E2750",
        "yAxisID": "y",
        "borderDash": [],
        "pointStyle": "circle"
      },
      {
        "label": "NSubstitute memory",
        "unit": "b",
        "data": [
          7928,
          7928,
          7928,
          7928,
          7928,
          7928,
          7928,
          7928,
          7928,
          7928,
          7928,
          7928,
          7928,
          7928
        ],
        "borderColor": "#5E2750",
        "backgroundColor": "#5E2750",
        "yAxisID": "y1",
        "borderDash": [
          5,
          5
        ],
        "pointStyle": "triangle"
      },
      {
        "label": "FakeItEasy time",
        "unit": "ns",
        "data": [
          4754.900245121547,
          4672.8294190626875,
          4383.440012105306,
          4884.810801369803,
          3764.064117685954,
          4837.0505447387695,
          4524.066829935709,
          4572.796613057454,
          4153.575600760324,
          4896.899882976825,
          4142.066209920247,
          4716.05678667341,
          4429.628007742075,
          4998.772673034668
        ],
        "borderColor": "#4A6FA5",
        "backgroundColor": "#4A6FA5",
        "yAxisID": "y",
        "borderDash": [],
        "pointStyle": "circle"
      },
      {
        "label": "FakeItEasy memory",
        "unit": "b",
        "data": [
          6970,
          6970,
          6970,
          6970,
          6970,
          6970,
          6959,
          6970,
          6970,
          6970,
          6970,
          6970,
          6959,
          6970
        ],
        "borderColor": "#4A6FA5",
        "backgroundColor": "#4A6FA5",
        "yAxisID": "y1",
        "borderDash": [
          5,
          5
        ],
        "pointStyle": "triangle"
      },
      {
        "label": "Imposter time",
        "unit": "ns",
        "data": [
          419.8464661916097,
          459.9864014943441,
          409.6093915190016,
          406.8710938181196,
          321.5161684989929,
          384.73958233424594,
          450.8619956970215,
          418.4747844696045,
          406.1658866746085,
          442.2074133872986,
          406.71004276275636,
          434.118200472423,
          487.4543059984843,
          430.4124413808187
        ],
        "borderColor": "#E84393",
        "backgroundColor": "#E84393",
        "yAxisID": "y",
        "borderDash": [],
        "pointStyle": "circle"
      },
      {
        "label": "Imposter memory",
        "unit": "b",
        "data": [
          2440,
          2440,
          2440,
          2440,
          2440,
          2440,
          2440,
          2440,
          2440,
          2440,
          2440,
          2440,
          2440,
          2440
        ],
        "borderColor": "#E84393",
        "backgroundColor": "#E84393",
        "yAxisID": "y1",
        "borderDash": [
          5,
          5
        ],
        "pointStyle": "triangle"
      },
      {
        "label": "TUnitMocks time",
        "unit": "ns",
        "data": [
          602.6764381408691,
          736.9003197987875,
          654.148777280535,
          589.7869474411011,
          538.4229355539594,
          702.9581379572551,
          705.9934259414673,
          655.1315539677938,
          604.4942819050381,
          619.8922446568807,
          624.7886574427287,
          621.6825705255781,
          749.7322867257254,
          625.4988119761149
        ],
        "borderColor": "#FF8C00",
        "backgroundColor": "#FF8C00",
        "yAxisID": "y",
        "borderDash": [],
        "pointStyle": "circle"
      },
      {
        "label": "TUnitMocks memory",
        "unit": "b",
        "data": [
          2688,
          2688,
          2688,
          2688,
          2688,
          2688,
          2688,
          2688,
          2688,
          2688,
          2688,
          2688,
          2688,
          2688
        ],
        "borderColor": "#FF8C00",
        "backgroundColor": "#FF8C00",
        "yAxisID": "y1",
        "borderDash": [
          5,
          5
        ],
        "pointStyle": "triangle"
      }
    ]
  },
  "Event": {
    "commits": [
      {
        "sha": "3eb0d03f222ccc5ac42a2f32e78546cc9c633150",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T13:26:28Z",
        "message": "feat: add baseline benchmark comparison to reporting (#744)"
      },
      {
        "sha": "381840acc2d2a841abbc64369aede5f364acdb1f",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T13:54:51Z",
        "message": "refactor: enhance benchmark table display names (#746)"
      },
      {
        "sha": "ace2d4ac2db5c68606ef27eb394c78f514e5557d",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T15:29:24Z",
        "message": "coverage: add build tests (#747)"
      },
      {
        "sha": "aac7d3511c8ac5e09b9d3ef995fc3b52c70b098a",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T15:52:53Z",
        "message": "perf: optimize generator-emitted setups by skipping dual registration (#741)"
      },
      {
        "sha": "c5047af16fa6992d7704cbfa089919c96091d884",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T16:25:14Z",
        "message": "fix: build error (#749)"
      },
      {
        "sha": "7f5a215d0dcc2c82a88f3fa2bd0aa7666848b9f8",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T16:39:28Z",
        "message": "perf: lazy-allocate per-member fast buffers on first record (#740)"
      },
      {
        "sha": "c28709e4647b9abe55894504efcf4d69941173cb",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T20:01:07Z",
        "message": "feat: bind \u0060Method(default, \u2026)\u0060 to the by-values overload (#750)"
      },
      {
        "sha": "200d0bb9827367185c99c06a7d05d7c9f84701bc",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T20:40:13Z",
        "message": "docs: update migration package description to include NSubstitute support (#751)"
      },
      {
        "sha": "64156009dafaca073bdfea9a5b04562dee1cc050",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-02T06:08:23Z",
        "message": "refactor: move source-generated mock to top of file (#752)"
      },
      {
        "sha": "681a2cfdd105fbed7fdd389922b2aee58bae897f",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-02T06:28:36Z",
        "message": "fix: make source generator snapshot build configuration invariant (#753)"
      },
      {
        "sha": "812a93f05fb7bbaf016d8b6e82a455a4270e591e",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-02T06:44:29Z",
        "message": "refactor: enable nullable reference types in BenchmarkReport (#754)"
      },
      {
        "sha": "a56e91e48833343e983992a69d2fe77b953b8f9c",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-02T20:56:04Z",
        "message": "docs: notify Testably.Site when documentation changes (#755)"
      },
      {
        "sha": "a3038970faba5546782c1657f6f0927ca5fcd930",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-02T22:10:02Z",
        "message": "refactor: update repository references from aweXpect to Testably (#756)"
      },
      {
        "sha": "557bcbc8ab11c6c1c0101656ed73d210511ba24b",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-03T05:56:25Z",
        "message": "fix: casing of sonar organization (#760)"
      }
    ],
    "labels": [
      "3eb0d03f",
      "381840ac",
      "ace2d4ac",
      "aac7d351",
      "c5047af1",
      "7f5a215d",
      "c28709e4",
      "200d0bb9",
      "64156009",
      "681a2cfd",
      "812a93f0",
      "a56e91e4",
      "a3038970",
      "557bcbc8"
    ],
    "datasets": [
      {
        "label": "Mockolate time",
        "unit": "ns",
        "data": [
          311.50117276509604,
          324.80877628326414,
          344.7411195755005,
          309.77829578944613,
          312.1151970545451,
          332.4974767611577,
          299.02867453893026,
          290.80444860458374,
          329.3417512689318,
          310.24626725060597,
          245.33923381169637,
          309.2253296216329,
          302.3985516684396,
          293.38783124514987
        ],
        "borderColor": "#63A2AC",
        "backgroundColor": "#63A2AC",
        "yAxisID": "y",
        "borderDash": [],
        "pointStyle": "circle"
      },
      {
        "label": "Mockolate memory",
        "unit": "b",
        "data": [
          1872,
          1872,
          1872,
          1952,
          1952,
          1824,
          1824,
          1824,
          1824,
          1824,
          1824,
          1824,
          1824,
          1824
        ],
        "borderColor": "#63A2AC",
        "backgroundColor": "#63A2AC",
        "yAxisID": "y1",
        "borderDash": [
          5,
          5
        ],
        "pointStyle": "triangle"
      },
      {
        "label": "Moq time",
        "unit": "ns",
        "data": [
          16604.932364327567,
          15888.823320661273,
          16421.674641927082,
          15713.618549053486,
          15935.912334735576,
          16129.071725027901,
          16067.30303485577,
          16094.150122070312,
          16278.928531901041,
          14069.879778180804,
          11181.401643880208,
          15998.8156476702,
          13854.295076497396,
          16389.613778250558
        ],
        "borderColor": "#A052B0",
        "backgroundColor": "#A052B0",
        "yAxisID": "y",
        "borderDash": [],
        "pointStyle": "circle"
      },
      {
        "label": "Moq memory",
        "unit": "b",
        "data": [
          12809,
          12809,
          12809,
          12809,
          12809,
          12809,
          12809,
          12809,
          12809,
          12809,
          12809,
          12809,
          12809,
          12809
        ],
        "borderColor": "#A052B0",
        "backgroundColor": "#A052B0",
        "yAxisID": "y1",
        "borderDash": [
          5,
          5
        ],
        "pointStyle": "triangle"
      },
      {
        "label": "NSubstitute time",
        "unit": "ns",
        "data": [
          5855.054614257812,
          5768.576793924967,
          5852.974615987142,
          5511.850238255092,
          5582.907276916504,
          5741.241499328613,
          5514.805159977504,
          5692.621219341571,
          5889.426822408041,
          5494.336930847168,
          4060.9170884352466,
          5762.507352193196,
          4970.815166219076,
          5524.319936116536
        ],
        "borderColor": "#5E2750",
        "backgroundColor": "#5E2750",
        "yAxisID": "y",
        "borderDash": [],
        "pointStyle": "circle"
      },
      {
        "label": "NSubstitute memory",
        "unit": "b",
        "data": [
          9264,
          9264,
          9264,
          9264,
          9264,
          9264,
          9264,
          9264,
          9264,
          9264,
          9264,
          9264,
          9264,
          9264
        ],
        "borderColor": "#5E2750",
        "backgroundColor": "#5E2750",
        "yAxisID": "y1",
        "borderDash": [
          5,
          5
        ],
        "pointStyle": "triangle"
      },
      {
        "label": "FakeItEasy time",
        "unit": "ns",
        "data": [
          216644.57858072917,
          213260.01793619792,
          213631.48018704928,
          212169.6768798828,
          211018.74705403644,
          214018.23053385416,
          210787.3561686198,
          212882.0942220052,
          219609.79431966145,
          231348.59015764509,
          183999.25136021205,
          214921.2912923177,
          230975.0235514323,
          213204.52139718193
        ],
        "borderColor": "#4A6FA5",
        "backgroundColor": "#4A6FA5",
        "yAxisID": "y",
        "borderDash": [],
        "pointStyle": "circle"
      },
      {
        "label": "FakeItEasy memory",
        "unit": "b",
        "data": [
          15907,
          15755,
          15755,
          15755,
          15755,
          15755,
          15628,
          15628,
          15628,
          15628,
          15628,
          15747,
          15628,
          15628
        ],
        "borderColor": "#4A6FA5",
        "backgroundColor": "#4A6FA5",
        "yAxisID": "y1",
        "borderDash": [
          5,
          5
        ],
        "pointStyle": "triangle"
      },
      {
        "label": "Imposter time",
        "unit": "ns",
        "data": [
          1648.344255765279,
          1457.500072479248,
          1360.1290839059013,
          1422.7209621429442,
          1330.7801713943481,
          1452.7029835837227,
          1310.8912282943725,
          1336.5912364959718,
          1469.5522981371198,
          1389.7432129723686,
          1066.8637983957926,
          1373.0015934535436,
          1339.760249546596,
          1311.4329317728677
        ],
        "borderColor": "#E84393",
        "backgroundColor": "#E84393",
        "yAxisID": "y",
        "borderDash": [],
        "pointStyle": "circle"
      },
      {
        "label": "Imposter memory",
        "unit": "b",
        "data": [
          9016,
          9016,
          9016,
          9016,
          9016,
          9016,
          9016,
          9016,
          9016,
          9016,
          9016,
          9016,
          9016,
          9016
        ],
        "borderColor": "#E84393",
        "backgroundColor": "#E84393",
        "yAxisID": "y1",
        "borderDash": [
          5,
          5
        ],
        "pointStyle": "triangle"
      },
      {
        "label": "TUnitMocks time",
        "unit": "ns",
        "data": [
          216.22692428032556,
          190.65735812187194,
          190.54728986535753,
          180.82671919235815,
          174.1192398254688,
          203.45939841270447,
          184.40220410029093,
          183.8825532436371,
          209.0734079360962,
          183.18945741653442,
          139.52175750051225,
          194.42035087517328,
          174.67472818692525,
          189.71946749687194
        ],
        "borderColor": "#FF8C00",
        "backgroundColor": "#FF8C00",
        "yAxisID": "y",
        "borderDash": [],
        "pointStyle": "circle"
      },
      {
        "label": "TUnitMocks memory",
        "unit": "b",
        "data": [
          1400,
          1400,
          1400,
          1400,
          1400,
          1400,
          1400,
          1400,
          1400,
          1400,
          1400,
          1400,
          1400,
          1400
        ],
        "borderColor": "#FF8C00",
        "backgroundColor": "#FF8C00",
        "yAxisID": "y1",
        "borderDash": [
          5,
          5
        ],
        "pointStyle": "triangle"
      }
    ]
  },
  "Indexer (N=1)": {
    "commits": [
      {
        "sha": "3eb0d03f222ccc5ac42a2f32e78546cc9c633150",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T13:26:28Z",
        "message": "feat: add baseline benchmark comparison to reporting (#744)"
      },
      {
        "sha": "381840acc2d2a841abbc64369aede5f364acdb1f",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T13:54:51Z",
        "message": "refactor: enhance benchmark table display names (#746)"
      },
      {
        "sha": "ace2d4ac2db5c68606ef27eb394c78f514e5557d",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T15:29:24Z",
        "message": "coverage: add build tests (#747)"
      },
      {
        "sha": "aac7d3511c8ac5e09b9d3ef995fc3b52c70b098a",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T15:52:53Z",
        "message": "perf: optimize generator-emitted setups by skipping dual registration (#741)"
      },
      {
        "sha": "c5047af16fa6992d7704cbfa089919c96091d884",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T16:25:14Z",
        "message": "fix: build error (#749)"
      },
      {
        "sha": "7f5a215d0dcc2c82a88f3fa2bd0aa7666848b9f8",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T16:39:28Z",
        "message": "perf: lazy-allocate per-member fast buffers on first record (#740)"
      },
      {
        "sha": "c28709e4647b9abe55894504efcf4d69941173cb",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T20:01:07Z",
        "message": "feat: bind \u0060Method(default, \u2026)\u0060 to the by-values overload (#750)"
      },
      {
        "sha": "200d0bb9827367185c99c06a7d05d7c9f84701bc",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T20:40:13Z",
        "message": "docs: update migration package description to include NSubstitute support (#751)"
      },
      {
        "sha": "64156009dafaca073bdfea9a5b04562dee1cc050",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-02T06:08:23Z",
        "message": "refactor: move source-generated mock to top of file (#752)"
      },
      {
        "sha": "681a2cfdd105fbed7fdd389922b2aee58bae897f",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-02T06:28:36Z",
        "message": "fix: make source generator snapshot build configuration invariant (#753)"
      },
      {
        "sha": "812a93f05fb7bbaf016d8b6e82a455a4270e591e",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-02T06:44:29Z",
        "message": "refactor: enable nullable reference types in BenchmarkReport (#754)"
      },
      {
        "sha": "a56e91e48833343e983992a69d2fe77b953b8f9c",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-02T20:56:04Z",
        "message": "docs: notify Testably.Site when documentation changes (#755)"
      },
      {
        "sha": "a3038970faba5546782c1657f6f0927ca5fcd930",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-02T22:10:02Z",
        "message": "refactor: update repository references from aweXpect to Testably (#756)"
      },
      {
        "sha": "557bcbc8ab11c6c1c0101656ed73d210511ba24b",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-03T05:56:25Z",
        "message": "fix: casing of sonar organization (#760)"
      }
    ],
    "labels": [
      "3eb0d03f",
      "381840ac",
      "ace2d4ac",
      "aac7d351",
      "c5047af1",
      "7f5a215d",
      "c28709e4",
      "200d0bb9",
      "64156009",
      "681a2cfd",
      "812a93f0",
      "a56e91e4",
      "a3038970",
      "557bcbc8"
    ],
    "datasets": [
      {
        "label": "Mockolate time",
        "unit": "ns",
        "data": [
          842.5106775420053,
          1069.6441811879477,
          903.103252337529,
          880.3614913304647,
          933.0362941878183,
          1027.6409844618577,
          1011.9585716247559,
          1034.5277795155844,
          904.5329016276768,
          935.5076890672956,
          825.5521801630656,
          916.286588873182,
          840.1738409042358,
          1066.4242071424212
        ],
        "borderColor": "#63A2AC",
        "backgroundColor": "#63A2AC",
        "yAxisID": "y",
        "borderDash": [],
        "pointStyle": "circle"
      },
      {
        "label": "Mockolate memory",
        "unit": "b",
        "data": [
          3992,
          3992,
          3992,
          3904,
          3904,
          3904,
          3904,
          3904,
          3904,
          3904,
          3904,
          3904,
          3904,
          3904
        ],
        "borderColor": "#63A2AC",
        "backgroundColor": "#63A2AC",
        "yAxisID": "y1",
        "borderDash": [
          5,
          5
        ],
        "pointStyle": "triangle"
      },
      {
        "label": "Moq time",
        "unit": "ns",
        "data": [
          134995.96808733259,
          167449.58515625,
          219668.94934895833,
          213131.4287109375,
          214441.3242563101,
          171488.12631460337,
          217577.5638997396,
          170082.23709542412,
          215500.61223493304,
          165984.53121744792,
          134234.4163248698,
          173250.2958984375,
          131275.57246907553,
          173923.29282924108
        ],
        "borderColor": "#A052B0",
        "backgroundColor": "#A052B0",
        "yAxisID": "y",
        "borderDash": [],
        "pointStyle": "circle"
      },
      {
        "label": "Moq memory",
        "unit": "b",
        "data": [
          20732,
          20849,
          20860,
          20980,
          20860,
          20860,
          20732,
          20972,
          20860,
          20860,
          20875,
          21084,
          20860,
          20732
        ],
        "borderColor": "#A052B0",
        "backgroundColor": "#A052B0",
        "yAxisID": "y1",
        "borderDash": [
          5,
          5
        ],
        "pointStyle": "triangle"
      },
      {
        "label": "NSubstitute time",
        "unit": "ns",
        "data": [
          6881.959520612444,
          9480.981268310547,
          9708.354034423828,
          9254.724938528878,
          9537.885803222656,
          8917.53256225586,
          9284.855974637545,
          8541.651309422085,
          9127.137467520577,
          8324.261111668178,
          6885.596005575998,
          9167.133240836007,
          6818.31262105306,
          9079.154235839844
        ],
        "borderColor": "#5E2750",
        "backgroundColor": "#5E2750",
        "yAxisID": "y",
        "borderDash": [],
        "pointStyle": "circle"
      },
      {
        "label": "NSubstitute memory",
        "unit": "b",
        "data": [
          13088,
          13088,
          13088,
          13144,
          13088,
          13144,
          13088,
          13088,
          13088,
          13088,
          13144,
          13144,
          13144,
          13144
        ],
        "borderColor": "#5E2750",
        "backgroundColor": "#5E2750",
        "yAxisID": "y1",
        "borderDash": [
          5,
          5
        ],
        "pointStyle": "triangle"
      },
      {
        "label": "FakeItEasy time",
        "unit": "ns",
        "data": [
          7956.168708292643,
          11003.92502339681,
          12260.517454020182,
          11199.15236722506,
          12207.300359453473,
          10249.89225362142,
          11884.947421482631,
          10420.526971435547,
          11822.085610525948,
          9394.56776537214,
          8015.271924845378,
          10395.524811808269,
          7837.4959187825525,
          10637.507712809245
        ],
        "borderColor": "#4A6FA5",
        "backgroundColor": "#4A6FA5",
        "yAxisID": "y",
        "borderDash": [],
        "pointStyle": "circle"
      },
      {
        "label": "FakeItEasy memory",
        "unit": "b",
        "data": [
          14211,
          14199,
          14211,
          14211,
          14211,
          14211,
          13954,
          14291,
          13954,
          13954,
          13954,
          13954,
          13954,
          14067
        ],
        "borderColor": "#4A6FA5",
        "backgroundColor": "#4A6FA5",
        "yAxisID": "y1",
        "borderDash": [
          5,
          5
        ],
        "pointStyle": "triangle"
      },
      {
        "label": "Imposter time",
        "unit": "ns",
        "data": [
          771.8797861735026,
          991.8016724904378,
          947.211189142863,
          897.3618002619062,
          982.2006805964878,
          949.8709081922259,
          946.410168393453,
          855.2149697031293,
          842.1067715372358,
          823.1063951492309,
          675.7019196919033,
          849.1345856530326,
          691.2715850976797,
          939.9056789534433
        ],
        "borderColor": "#E84393",
        "backgroundColor": "#E84393",
        "yAxisID": "y",
        "borderDash": [],
        "pointStyle": "circle"
      },
      {
        "label": "Imposter memory",
        "unit": "b",
        "data": [
          5280,
          5280,
          5280,
          5280,
          5280,
          5280,
          5280,
          5280,
          5280,
          5280,
          5280,
          5280,
          5280,
          5280
        ],
        "borderColor": "#E84393",
        "backgroundColor": "#E84393",
        "yAxisID": "y1",
        "borderDash": [
          5,
          5
        ],
        "pointStyle": "triangle"
      }
    ]
  },
  "Indexer (N=10)": {
    "commits": [
      {
        "sha": "3eb0d03f222ccc5ac42a2f32e78546cc9c633150",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T13:26:28Z",
        "message": "feat: add baseline benchmark comparison to reporting (#744)"
      },
      {
        "sha": "381840acc2d2a841abbc64369aede5f364acdb1f",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T13:54:51Z",
        "message": "refactor: enhance benchmark table display names (#746)"
      },
      {
        "sha": "ace2d4ac2db5c68606ef27eb394c78f514e5557d",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T15:29:24Z",
        "message": "coverage: add build tests (#747)"
      },
      {
        "sha": "aac7d3511c8ac5e09b9d3ef995fc3b52c70b098a",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T15:52:53Z",
        "message": "perf: optimize generator-emitted setups by skipping dual registration (#741)"
      },
      {
        "sha": "c5047af16fa6992d7704cbfa089919c96091d884",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T16:25:14Z",
        "message": "fix: build error (#749)"
      },
      {
        "sha": "7f5a215d0dcc2c82a88f3fa2bd0aa7666848b9f8",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T16:39:28Z",
        "message": "perf: lazy-allocate per-member fast buffers on first record (#740)"
      },
      {
        "sha": "c28709e4647b9abe55894504efcf4d69941173cb",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T20:01:07Z",
        "message": "feat: bind \u0060Method(default, \u2026)\u0060 to the by-values overload (#750)"
      },
      {
        "sha": "200d0bb9827367185c99c06a7d05d7c9f84701bc",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T20:40:13Z",
        "message": "docs: update migration package description to include NSubstitute support (#751)"
      },
      {
        "sha": "64156009dafaca073bdfea9a5b04562dee1cc050",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-02T06:08:23Z",
        "message": "refactor: move source-generated mock to top of file (#752)"
      },
      {
        "sha": "681a2cfdd105fbed7fdd389922b2aee58bae897f",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-02T06:28:36Z",
        "message": "fix: make source generator snapshot build configuration invariant (#753)"
      },
      {
        "sha": "812a93f05fb7bbaf016d8b6e82a455a4270e591e",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-02T06:44:29Z",
        "message": "refactor: enable nullable reference types in BenchmarkReport (#754)"
      },
      {
        "sha": "a56e91e48833343e983992a69d2fe77b953b8f9c",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-02T20:56:04Z",
        "message": "docs: notify Testably.Site when documentation changes (#755)"
      },
      {
        "sha": "a3038970faba5546782c1657f6f0927ca5fcd930",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-02T22:10:02Z",
        "message": "refactor: update repository references from aweXpect to Testably (#756)"
      },
      {
        "sha": "557bcbc8ab11c6c1c0101656ed73d210511ba24b",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-03T05:56:25Z",
        "message": "fix: casing of sonar organization (#760)"
      }
    ],
    "labels": [
      "3eb0d03f",
      "381840ac",
      "ace2d4ac",
      "aac7d351",
      "c5047af1",
      "7f5a215d",
      "c28709e4",
      "200d0bb9",
      "64156009",
      "681a2cfd",
      "812a93f0",
      "a56e91e4",
      "a3038970",
      "557bcbc8"
    ],
    "datasets": [
      {
        "label": "Mockolate time",
        "unit": "ns",
        "data": [
          1958.1845428466797,
          2968.4180270603724,
          2434.491397857666,
          2403.56270129864,
          2494.4537086486816,
          2587.3538556780136,
          2706.630377960205,
          2542.271853383382,
          2381.700254567464,
          2381.1519590524526,
          2044.9140973409017,
          2561.8957685743057,
          2081.1258931477864,
          2653.004323225755
        ],
        "borderColor": "#63A2AC",
        "backgroundColor": "#63A2AC",
        "yAxisID": "y",
        "borderDash": [],
        "pointStyle": "circle"
      },
      {
        "label": "Mockolate memory",
        "unit": "b",
        "data": [
          5072,
          5072,
          5072,
          4984,
          4984,
          4984,
          4984,
          4984,
          4984,
          4984,
          4984,
          4984,
          4984,
          4984
        ],
        "borderColor": "#63A2AC",
        "backgroundColor": "#63A2AC",
        "yAxisID": "y1",
        "borderDash": [
          5,
          5
        ],
        "pointStyle": "triangle"
      },
      {
        "label": "Moq time",
        "unit": "ns",
        "data": [
          143025.88999023437,
          178482.97001953126,
          227934.87041364398,
          227241.37915039062,
          227593.3686035156,
          185177.29134114584,
          229589.08804757256,
          184168.02673339844,
          225253.28889347956,
          177573.5818684896,
          142187.07052176338,
          185280.19864908853,
          140860.1557779948,
          187425.79802594866
        ],
        "borderColor": "#A052B0",
        "backgroundColor": "#A052B0",
        "yAxisID": "y",
        "borderDash": [],
        "pointStyle": "circle"
      },
      {
        "label": "Moq memory",
        "unit": "b",
        "data": [
          29332,
          30603,
          30610,
          30730,
          30610,
          30610,
          29332,
          30722,
          30610,
          30610,
          30610,
          30834,
          30610,
          29332
        ],
        "borderColor": "#A052B0",
        "backgroundColor": "#A052B0",
        "yAxisID": "y1",
        "borderDash": [
          5,
          5
        ],
        "pointStyle": "triangle"
      },
      {
        "label": "NSubstitute time",
        "unit": "ns",
        "data": [
          17052.993512834822,
          22527.54294433594,
          22849.077794392902,
          22322.993927001953,
          23604.294201660156,
          22861.015675136023,
          22771.313001360213,
          22712.983968098957,
          22022.666353352866,
          20563.821066720146,
          17123.549006535457,
          22663.70271083287,
          16925.975540161133,
          22719.5464390346
        ],
        "borderColor": "#5E2750",
        "backgroundColor": "#5E2750",
        "yAxisID": "y",
        "borderDash": [],
        "pointStyle": "circle"
      },
      {
        "label": "NSubstitute memory",
        "unit": "b",
        "data": [
          26193,
          26193,
          26193,
          26249,
          26193,
          26249,
          26193,
          26193,
          26193,
          26193,
          26249,
          26249,
          26249,
          26249
        ],
        "borderColor": "#5E2750",
        "backgroundColor": "#5E2750",
        "yAxisID": "y1",
        "borderDash": [
          5,
          5
        ],
        "pointStyle": "triangle"
      },
      {
        "label": "FakeItEasy time",
        "unit": "ns",
        "data": [
          17197.557603102465,
          23890.90993172782,
          25817.673666817802,
          24160.010746547156,
          25188.281955973307,
          22661.51159464518,
          24895.5771870931,
          21297.29385593959,
          24328.535712105888,
          20522.382226126534,
          17004.59439086914,
          21709.052355957032,
          16719.049741472518,
          22547.945922851562
        ],
        "borderColor": "#4A6FA5",
        "backgroundColor": "#4A6FA5",
        "yAxisID": "y",
        "borderDash": [],
        "pointStyle": "circle"
      },
      {
        "label": "FakeItEasy memory",
        "unit": "b",
        "data": [
          36325,
          36333,
          36325,
          36325,
          36325,
          36325,
          33764,
          34101,
          33764,
          33764,
          33764,
          33764,
          33764,
          33877
        ],
        "borderColor": "#4A6FA5",
        "backgroundColor": "#4A6FA5",
        "yAxisID": "y1",
        "borderDash": [
          5,
          5
        ],
        "pointStyle": "triangle"
      },
      {
        "label": "Imposter time",
        "unit": "ns",
        "data": [
          1745.553673199245,
          2349.284425608317,
          2164.87181447347,
          2235.9926775418794,
          2245.418864695231,
          2285.225606536865,
          2146.841716003418,
          2117.4403893607005,
          2031.6487666538783,
          1981.3443625313896,
          1664.0328286034721,
          2199.339442033034,
          1701.219325129191,
          2252.872452799479
        ],
        "borderColor": "#E84393",
        "backgroundColor": "#E84393",
        "yAxisID": "y",
        "borderDash": [],
        "pointStyle": "circle"
      },
      {
        "label": "Imposter memory",
        "unit": "b",
        "data": [
          8160,
          8160,
          8160,
          8160,
          8160,
          8160,
          8160,
          8160,
          8160,
          8160,
          8160,
          8160,
          8160,
          8160
        ],
        "borderColor": "#E84393",
        "backgroundColor": "#E84393",
        "yAxisID": "y1",
        "borderDash": [
          5,
          5
        ],
        "pointStyle": "triangle"
      }
    ]
  },
  "Method (N=1)": {
    "commits": [
      {
        "sha": "3eb0d03f222ccc5ac42a2f32e78546cc9c633150",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T13:26:28Z",
        "message": "feat: add baseline benchmark comparison to reporting (#744)"
      },
      {
        "sha": "381840acc2d2a841abbc64369aede5f364acdb1f",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T13:54:51Z",
        "message": "refactor: enhance benchmark table display names (#746)"
      },
      {
        "sha": "ace2d4ac2db5c68606ef27eb394c78f514e5557d",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T15:29:24Z",
        "message": "coverage: add build tests (#747)"
      },
      {
        "sha": "aac7d3511c8ac5e09b9d3ef995fc3b52c70b098a",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T15:52:53Z",
        "message": "perf: optimize generator-emitted setups by skipping dual registration (#741)"
      },
      {
        "sha": "c5047af16fa6992d7704cbfa089919c96091d884",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T16:25:14Z",
        "message": "fix: build error (#749)"
      },
      {
        "sha": "7f5a215d0dcc2c82a88f3fa2bd0aa7666848b9f8",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T16:39:28Z",
        "message": "perf: lazy-allocate per-member fast buffers on first record (#740)"
      },
      {
        "sha": "c28709e4647b9abe55894504efcf4d69941173cb",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T20:01:07Z",
        "message": "feat: bind \u0060Method(default, \u2026)\u0060 to the by-values overload (#750)"
      },
      {
        "sha": "200d0bb9827367185c99c06a7d05d7c9f84701bc",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T20:40:13Z",
        "message": "docs: update migration package description to include NSubstitute support (#751)"
      },
      {
        "sha": "64156009dafaca073bdfea9a5b04562dee1cc050",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-02T06:08:23Z",
        "message": "refactor: move source-generated mock to top of file (#752)"
      },
      {
        "sha": "681a2cfdd105fbed7fdd389922b2aee58bae897f",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-02T06:28:36Z",
        "message": "fix: make source generator snapshot build configuration invariant (#753)"
      },
      {
        "sha": "812a93f05fb7bbaf016d8b6e82a455a4270e591e",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-02T06:44:29Z",
        "message": "refactor: enable nullable reference types in BenchmarkReport (#754)"
      },
      {
        "sha": "a56e91e48833343e983992a69d2fe77b953b8f9c",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-02T20:56:04Z",
        "message": "docs: notify Testably.Site when documentation changes (#755)"
      },
      {
        "sha": "a3038970faba5546782c1657f6f0927ca5fcd930",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-02T22:10:02Z",
        "message": "refactor: update repository references from aweXpect to Testably (#756)"
      },
      {
        "sha": "557bcbc8ab11c6c1c0101656ed73d210511ba24b",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-03T05:56:25Z",
        "message": "fix: casing of sonar organization (#760)"
      }
    ],
    "labels": [
      "3eb0d03f",
      "381840ac",
      "ace2d4ac",
      "aac7d351",
      "c5047af1",
      "7f5a215d",
      "c28709e4",
      "200d0bb9",
      "64156009",
      "681a2cfd",
      "812a93f0",
      "a56e91e4",
      "a3038970",
      "557bcbc8"
    ],
    "datasets": [
      {
        "label": "Mockolate time",
        "unit": "ns",
        "data": [
          427.80603726704913,
          419.6126798561641,
          456.41962842941285,
          360.8230167168837,
          382.78139743804934,
          352.8897578899677,
          403.4852691968282,
          395.0266225337982,
          412.2879053751628,
          407.2088309923808,
          398.0072230952127,
          358.91345981451184,
          345.0845811707633,
          398.25492668151855
        ],
        "borderColor": "#63A2AC",
        "backgroundColor": "#63A2AC",
        "yAxisID": "y",
        "borderDash": [],
        "pointStyle": "circle"
      },
      {
        "label": "Mockolate memory",
        "unit": "b",
        "data": [
          2200,
          2200,
          2200,
          2088,
          2088,
          2088,
          2088,
          2088,
          2088,
          2088,
          2088,
          2088,
          2088,
          2088
        ],
        "borderColor": "#63A2AC",
        "backgroundColor": "#63A2AC",
        "yAxisID": "y1",
        "borderDash": [
          5,
          5
        ],
        "pointStyle": "triangle"
      },
      {
        "label": "Moq time",
        "unit": "ns",
        "data": [
          131590.8612905649,
          130710.16692243304,
          134776.68087332588,
          130068.40666316106,
          185743.65071614584,
          180607.3369891827,
          134752.72986778847,
          187164.47126652644,
          184460.30399576822,
          182793.35648018974,
          140440.2296875,
          131557.37789481026,
          178977.00105794272,
          134101.3220027043
        ],
        "borderColor": "#A052B0",
        "backgroundColor": "#A052B0",
        "yAxisID": "y",
        "borderDash": [],
        "pointStyle": "circle"
      },
      {
        "label": "Moq memory",
        "unit": "b",
        "data": [
          14926,
          14926,
          14940,
          14926,
          14926,
          14926,
          15086,
          15086,
          14926,
          14926,
          15162,
          15086,
          14926,
          15098
        ],
        "borderColor": "#A052B0",
        "backgroundColor": "#A052B0",
        "yAxisID": "y1",
        "borderDash": [
          5,
          5
        ],
        "pointStyle": "triangle"
      },
      {
        "label": "NSubstitute time",
        "unit": "ns",
        "data": [
          5350.79166208903,
          5290.396083577474,
          5637.137350972494,
          5358.1653480529785,
          5897.2284703572595,
          5594.77348022461,
          5511.388178311861,
          5991.813898213704,
          5955.822418212891,
          5784.680227007185,
          5669.715598042806,
          5209.717715454101,
          5609.6067301432295,
          5447.337833658854
        ],
        "borderColor": "#5E2750",
        "backgroundColor": "#5E2750",
        "yAxisID": "y",
        "borderDash": [],
        "pointStyle": "circle"
      },
      {
        "label": "NSubstitute memory",
        "unit": "b",
        "data": [
          9336,
          9336,
          9280,
          9336,
          9280,
          9336,
          9336,
          9336,
          9336,
          9336,
          9336,
          9336,
          9336,
          9336
        ],
        "borderColor": "#5E2750",
        "backgroundColor": "#5E2750",
        "yAxisID": "y1",
        "borderDash": [
          5,
          5
        ],
        "pointStyle": "triangle"
      },
      {
        "label": "FakeItEasy time",
        "unit": "ns",
        "data": [
          5467.704341125488,
          5347.612500508626,
          5924.867851257324,
          5663.2555943806965,
          6249.617050170898,
          5975.069914245605,
          5538.620776367187,
          5979.473212650844,
          6506.609594726562,
          6088.547815050398,
          5815.628039042155,
          5276.2354992457795,
          6016.195068359375,
          5414.452039991106
        ],
        "borderColor": "#4A6FA5",
        "backgroundColor": "#4A6FA5",
        "yAxisID": "y",
        "borderDash": [],
        "pointStyle": "circle"
      },
      {
        "label": "FakeItEasy memory",
        "unit": "b",
        "data": [
          8309,
          8309,
          8316,
          8309,
          8309,
          8309,
          8244,
          8244,
          8244,
          8244,
          8251,
          8245,
          8244,
          8244
        ],
        "borderColor": "#4A6FA5",
        "backgroundColor": "#4A6FA5",
        "yAxisID": "y1",
        "borderDash": [
          5,
          5
        ],
        "pointStyle": "triangle"
      },
      {
        "label": "Imposter time",
        "unit": "ns",
        "data": [
          547.1992684773037,
          541.8106062571208,
          652.0549636205037,
          594.0153950963702,
          639.7951902662005,
          565.3008678981236,
          532.928661278316,
          609.5968976020813,
          659.2077004114786,
          600.6144524354202,
          638.1174445519081,
          533.6110844612122,
          544.3189567838397,
          560.5418188730875
        ],
        "borderColor": "#E84393",
        "backgroundColor": "#E84393",
        "yAxisID": "y",
        "borderDash": [],
        "pointStyle": "circle"
      },
      {
        "label": "Imposter memory",
        "unit": "b",
        "data": [
          4136,
          4136,
          4136,
          4136,
          4136,
          4136,
          4136,
          4136,
          4136,
          4136,
          4136,
          4136,
          4136,
          4136
        ],
        "borderColor": "#E84393",
        "backgroundColor": "#E84393",
        "yAxisID": "y1",
        "borderDash": [
          5,
          5
        ],
        "pointStyle": "triangle"
      },
      {
        "label": "TUnitMocks time",
        "unit": "ns",
        "data": [
          693.9968569278717,
          661.5510896046957,
          738.087799685342,
          742.8957029751369,
          712.1499548594157,
          664.2763502938407,
          679.5081852595011,
          704.8012968063355,
          728.189561398824,
          689.3500640732901,
          761.0241287867228,
          699.1745161328997,
          630.0839758555095,
          741.9827230453491
        ],
        "borderColor": "#FF8C00",
        "backgroundColor": "#FF8C00",
        "yAxisID": "y",
        "borderDash": [],
        "pointStyle": "circle"
      },
      {
        "label": "TUnitMocks memory",
        "unit": "b",
        "data": [
          2968,
          2968,
          2968,
          2968,
          2968,
          2968,
          2968,
          2968,
          2968,
          2968,
          2968,
          2968,
          2968,
          2968
        ],
        "borderColor": "#FF8C00",
        "backgroundColor": "#FF8C00",
        "yAxisID": "y1",
        "borderDash": [
          5,
          5
        ],
        "pointStyle": "triangle"
      }
    ]
  },
  "Method (N=10)": {
    "commits": [
      {
        "sha": "3eb0d03f222ccc5ac42a2f32e78546cc9c633150",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T13:26:28Z",
        "message": "feat: add baseline benchmark comparison to reporting (#744)"
      },
      {
        "sha": "381840acc2d2a841abbc64369aede5f364acdb1f",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T13:54:51Z",
        "message": "refactor: enhance benchmark table display names (#746)"
      },
      {
        "sha": "ace2d4ac2db5c68606ef27eb394c78f514e5557d",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T15:29:24Z",
        "message": "coverage: add build tests (#747)"
      },
      {
        "sha": "aac7d3511c8ac5e09b9d3ef995fc3b52c70b098a",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T15:52:53Z",
        "message": "perf: optimize generator-emitted setups by skipping dual registration (#741)"
      },
      {
        "sha": "c5047af16fa6992d7704cbfa089919c96091d884",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T16:25:14Z",
        "message": "fix: build error (#749)"
      },
      {
        "sha": "7f5a215d0dcc2c82a88f3fa2bd0aa7666848b9f8",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T16:39:28Z",
        "message": "perf: lazy-allocate per-member fast buffers on first record (#740)"
      },
      {
        "sha": "c28709e4647b9abe55894504efcf4d69941173cb",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T20:01:07Z",
        "message": "feat: bind \u0060Method(default, \u2026)\u0060 to the by-values overload (#750)"
      },
      {
        "sha": "200d0bb9827367185c99c06a7d05d7c9f84701bc",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T20:40:13Z",
        "message": "docs: update migration package description to include NSubstitute support (#751)"
      },
      {
        "sha": "64156009dafaca073bdfea9a5b04562dee1cc050",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-02T06:08:23Z",
        "message": "refactor: move source-generated mock to top of file (#752)"
      },
      {
        "sha": "681a2cfdd105fbed7fdd389922b2aee58bae897f",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-02T06:28:36Z",
        "message": "fix: make source generator snapshot build configuration invariant (#753)"
      },
      {
        "sha": "812a93f05fb7bbaf016d8b6e82a455a4270e591e",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-02T06:44:29Z",
        "message": "refactor: enable nullable reference types in BenchmarkReport (#754)"
      },
      {
        "sha": "a56e91e48833343e983992a69d2fe77b953b8f9c",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-02T20:56:04Z",
        "message": "docs: notify Testably.Site when documentation changes (#755)"
      },
      {
        "sha": "a3038970faba5546782c1657f6f0927ca5fcd930",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-02T22:10:02Z",
        "message": "refactor: update repository references from aweXpect to Testably (#756)"
      },
      {
        "sha": "557bcbc8ab11c6c1c0101656ed73d210511ba24b",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-03T05:56:25Z",
        "message": "fix: casing of sonar organization (#760)"
      }
    ],
    "labels": [
      "3eb0d03f",
      "381840ac",
      "ace2d4ac",
      "aac7d351",
      "c5047af1",
      "7f5a215d",
      "c28709e4",
      "200d0bb9",
      "64156009",
      "681a2cfd",
      "812a93f0",
      "a56e91e4",
      "a3038970",
      "557bcbc8"
    ],
    "datasets": [
      {
        "label": "Mockolate time",
        "unit": "ns",
        "data": [
          710.7035261789957,
          684.7676672935486,
          898.06706199646,
          645.4335536956787,
          737.6658861796061,
          651.8142990384783,
          660.4669244766235,
          668.6511014938354,
          725.67510502155,
          721.9957804997762,
          846.81719678243,
          641.4946570078532,
          645.6327701715322,
          651.3921939304897
        ],
        "borderColor": "#63A2AC",
        "backgroundColor": "#63A2AC",
        "yAxisID": "y",
        "borderDash": [],
        "pointStyle": "circle"
      },
      {
        "label": "Mockolate memory",
        "unit": "b",
        "data": [
          2416,
          2416,
          2416,
          2304,
          2304,
          2304,
          2304,
          2304,
          2304,
          2304,
          2304,
          2304,
          2304,
          2304
        ],
        "borderColor": "#63A2AC",
        "backgroundColor": "#63A2AC",
        "yAxisID": "y1",
        "borderDash": [
          5,
          5
        ],
        "pointStyle": "triangle"
      },
      {
        "label": "Moq time",
        "unit": "ns",
        "data": [
          134497.40420297475,
          136995.31967773437,
          141282.92587890624,
          136760.1986653646,
          193885.96805513822,
          185087.46774088542,
          141501.82151576452,
          188731.6629231771,
          191732.81677246094,
          187149.01108022837,
          146625.40698242188,
          136998.67348632813,
          184408.24147251673,
          139343.5736741286
        ],
        "borderColor": "#A052B0",
        "backgroundColor": "#A052B0",
        "yAxisID": "y",
        "borderDash": [],
        "pointStyle": "circle"
      },
      {
        "label": "Moq memory",
        "unit": "b",
        "data": [
          18925,
          18925,
          18929,
          18924,
          18925,
          18925,
          19085,
          19085,
          18925,
          18925,
          19153,
          19085,
          18925,
          19085
        ],
        "borderColor": "#A052B0",
        "backgroundColor": "#A052B0",
        "yAxisID": "y1",
        "borderDash": [
          5,
          5
        ],
        "pointStyle": "triangle"
      },
      {
        "label": "NSubstitute time",
        "unit": "ns",
        "data": [
          7932.632070922851,
          7836.20796661377,
          8054.984147208078,
          8306.834645080566,
          8850.276189531598,
          8468.933479309082,
          8033.106710815429,
          8714.188052586147,
          8965.253504071918,
          8736.461755371094,
          8446.851904296875,
          7846.050793457031,
          8392.460829598564,
          7980.104485066732
        ],
        "borderColor": "#5E2750",
        "backgroundColor": "#5E2750",
        "yAxisID": "y",
        "borderDash": [],
        "pointStyle": "circle"
      },
      {
        "label": "NSubstitute memory",
        "unit": "b",
        "data": [
          12361,
          12361,
          11800,
          12361,
          11800,
          12361,
          12361,
          12361,
          12361,
          12361,
          12360,
          12361,
          12361,
          12361
        ],
        "borderColor": "#5E2750",
        "backgroundColor": "#5E2750",
        "yAxisID": "y1",
        "borderDash": [
          5,
          5
        ],
        "pointStyle": "triangle"
      },
      {
        "label": "FakeItEasy time",
        "unit": "ns",
        "data": [
          8488.345676676432,
          8835.892796443059,
          9586.205005900065,
          8599.5991452535,
          10585.543349202473,
          9259.052262878418,
          8716.374763997395,
          9273.873670305524,
          9997.992697143554,
          9480.088179524739,
          9431.415278116861,
          8293.659459431967,
          9332.329572550456,
          8468.127075195312
        ],
        "borderColor": "#4A6FA5",
        "backgroundColor": "#4A6FA5",
        "yAxisID": "y",
        "borderDash": [],
        "pointStyle": "circle"
      },
      {
        "label": "FakeItEasy memory",
        "unit": "b",
        "data": [
          16433,
          16433,
          16428,
          16433,
          16433,
          16433,
          15786,
          15786,
          15786,
          15786,
          15786,
          15786,
          15786,
          15786
        ],
        "borderColor": "#4A6FA5",
        "backgroundColor": "#4A6FA5",
        "yAxisID": "y1",
        "borderDash": [
          5,
          5
        ],
        "pointStyle": "triangle"
      },
      {
        "label": "Imposter time",
        "unit": "ns",
        "data": [
          1128.1073453085762,
          1146.637362798055,
          1276.4043176015218,
          1179.9594188417707,
          1428.2198314666748,
          1217.120736530849,
          1120.46076520284,
          1126.6244450887045,
          1339.9322468893868,
          1161.215601094564,
          1238.1567395528157,
          1076.532220586141,
          1093.0878331320625,
          1090.9280326025826
        ],
        "borderColor": "#E84393",
        "backgroundColor": "#E84393",
        "yAxisID": "y",
        "borderDash": [],
        "pointStyle": "circle"
      },
      {
        "label": "Imposter memory",
        "unit": "b",
        "data": [
          5648,
          5648,
          5648,
          5648,
          5648,
          5648,
          5648,
          5648,
          5648,
          5648,
          5648,
          5648,
          5648,
          5648
        ],
        "borderColor": "#E84393",
        "backgroundColor": "#E84393",
        "yAxisID": "y1",
        "borderDash": [
          5,
          5
        ],
        "pointStyle": "triangle"
      },
      {
        "label": "TUnitMocks time",
        "unit": "ns",
        "data": [
          1829.3045738220214,
          1748.5735580444336,
          2131.4431935628254,
          1789.8728347191443,
          1817.5130151112874,
          1683.761801447187,
          1857.532373301188,
          1678.6495368140083,
          1777.4666797931377,
          1735.865975443522,
          2164.7974047342937,
          1778.6807876314435,
          1633.698813365056,
          1780.437987391154
        ],
        "borderColor": "#FF8C00",
        "backgroundColor": "#FF8C00",
        "yAxisID": "y",
        "borderDash": [],
        "pointStyle": "circle"
      },
      {
        "label": "TUnitMocks memory",
        "unit": "b",
        "data": [
          4600,
          4600,
          4600,
          4600,
          4600,
          4600,
          4600,
          4600,
          4600,
          4600,
          4600,
          4600,
          4600,
          4600
        ],
        "borderColor": "#FF8C00",
        "backgroundColor": "#FF8C00",
        "yAxisID": "y1",
        "borderDash": [
          5,
          5
        ],
        "pointStyle": "triangle"
      }
    ]
  },
  "Property (N=1)": {
    "commits": [
      {
        "sha": "3eb0d03f222ccc5ac42a2f32e78546cc9c633150",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T13:26:28Z",
        "message": "feat: add baseline benchmark comparison to reporting (#744)"
      },
      {
        "sha": "381840acc2d2a841abbc64369aede5f364acdb1f",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T13:54:51Z",
        "message": "refactor: enhance benchmark table display names (#746)"
      },
      {
        "sha": "ace2d4ac2db5c68606ef27eb394c78f514e5557d",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T15:29:24Z",
        "message": "coverage: add build tests (#747)"
      },
      {
        "sha": "aac7d3511c8ac5e09b9d3ef995fc3b52c70b098a",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T15:52:53Z",
        "message": "perf: optimize generator-emitted setups by skipping dual registration (#741)"
      },
      {
        "sha": "c5047af16fa6992d7704cbfa089919c96091d884",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T16:25:14Z",
        "message": "fix: build error (#749)"
      },
      {
        "sha": "7f5a215d0dcc2c82a88f3fa2bd0aa7666848b9f8",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T16:39:28Z",
        "message": "perf: lazy-allocate per-member fast buffers on first record (#740)"
      },
      {
        "sha": "c28709e4647b9abe55894504efcf4d69941173cb",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T20:01:07Z",
        "message": "feat: bind \u0060Method(default, \u2026)\u0060 to the by-values overload (#750)"
      },
      {
        "sha": "200d0bb9827367185c99c06a7d05d7c9f84701bc",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T20:40:13Z",
        "message": "docs: update migration package description to include NSubstitute support (#751)"
      },
      {
        "sha": "64156009dafaca073bdfea9a5b04562dee1cc050",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-02T06:08:23Z",
        "message": "refactor: move source-generated mock to top of file (#752)"
      },
      {
        "sha": "681a2cfdd105fbed7fdd389922b2aee58bae897f",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-02T06:28:36Z",
        "message": "fix: make source generator snapshot build configuration invariant (#753)"
      },
      {
        "sha": "812a93f05fb7bbaf016d8b6e82a455a4270e591e",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-02T06:44:29Z",
        "message": "refactor: enable nullable reference types in BenchmarkReport (#754)"
      },
      {
        "sha": "a56e91e48833343e983992a69d2fe77b953b8f9c",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-02T20:56:04Z",
        "message": "docs: notify Testably.Site when documentation changes (#755)"
      },
      {
        "sha": "a3038970faba5546782c1657f6f0927ca5fcd930",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-02T22:10:02Z",
        "message": "refactor: update repository references from aweXpect to Testably (#756)"
      },
      {
        "sha": "557bcbc8ab11c6c1c0101656ed73d210511ba24b",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-03T05:56:25Z",
        "message": "fix: casing of sonar organization (#760)"
      }
    ],
    "labels": [
      "3eb0d03f",
      "381840ac",
      "ace2d4ac",
      "aac7d351",
      "c5047af1",
      "7f5a215d",
      "c28709e4",
      "200d0bb9",
      "64156009",
      "681a2cfd",
      "812a93f0",
      "a56e91e4",
      "a3038970",
      "557bcbc8"
    ],
    "datasets": [
      {
        "label": "Mockolate time",
        "unit": "ns",
        "data": [
          537.4692910512289,
          504.9579170227051,
          623.5359698613485,
          489.6481078954843,
          618.0274983133588,
          549.4980311711629,
          580.4694944109235,
          512.0791901906331,
          538.2236106872558,
          502.152477484483,
          501.90197575887044,
          509.02407499949135,
          525.4557540893554,
          562.7049272537231
        ],
        "borderColor": "#63A2AC",
        "backgroundColor": "#63A2AC",
        "yAxisID": "y",
        "borderDash": [],
        "pointStyle": "circle"
      },
      {
        "label": "Mockolate memory",
        "unit": "b",
        "data": [
          2520,
          2520,
          2520,
          2520,
          2520,
          2520,
          2520,
          2520,
          2520,
          2520,
          2520,
          2520,
          2520,
          2520
        ],
        "borderColor": "#63A2AC",
        "backgroundColor": "#63A2AC",
        "yAxisID": "y1",
        "borderDash": [
          5,
          5
        ],
        "pointStyle": "triangle"
      },
      {
        "label": "Moq time",
        "unit": "ns",
        "data": [
          9891.59597342355,
          12099.936539713543,
          12556.833618164062,
          11752.535975864956,
          11152.099993024554,
          10916.56283569336,
          12252.99296875,
          11860.290669759115,
          12136.248783656529,
          12437.499661959135,
          11738.021268404447,
          12201.798311360677,
          11858.561564127604,
          11472.629813639323
        ],
        "borderColor": "#A052B0",
        "backgroundColor": "#A052B0",
        "yAxisID": "y",
        "borderDash": [],
        "pointStyle": "circle"
      },
      {
        "label": "Moq memory",
        "unit": "b",
        "data": [
          10513,
          10514,
          10641,
          10641,
          10641,
          10641,
          10753,
          10641,
          10641,
          10641,
          10513,
          10721,
          10513,
          10721
        ],
        "borderColor": "#A052B0",
        "backgroundColor": "#A052B0",
        "yAxisID": "y1",
        "borderDash": [
          5,
          5
        ],
        "pointStyle": "triangle"
      },
      {
        "label": "NSubstitute time",
        "unit": "ns",
        "data": [
          6959.571132405599,
          7639.001329694475,
          8137.808023071289,
          7575.339728800455,
          7582.229884847005,
          7028.861791992187,
          7461.0893280029295,
          7411.103737894694,
          7769.263819013323,
          7460.786527361189,
          7209.767017364502,
          7463.74459177653,
          7208.508314405169,
          7566.912901306152
        ],
        "borderColor": "#5E2750",
        "backgroundColor": "#5E2750",
        "yAxisID": "y",
        "borderDash": [],
        "pointStyle": "circle"
      },
      {
        "label": "NSubstitute memory",
        "unit": "b",
        "data": [
          11720,
          11720,
          11720,
          11720,
          11720,
          11720,
          11720,
          11720,
          11720,
          11720,
          11720,
          11720,
          11720,
          11720
        ],
        "borderColor": "#5E2750",
        "backgroundColor": "#5E2750",
        "yAxisID": "y1",
        "borderDash": [
          5,
          5
        ],
        "pointStyle": "triangle"
      },
      {
        "label": "FakeItEasy time",
        "unit": "ns",
        "data": [
          7076.000939941407,
          8308.292001577523,
          8980.453291829426,
          8435.08640725272,
          7862.431936645507,
          8186.362021891276,
          8793.699717203775,
          8179.4737263997395,
          8445.124872843424,
          8430.060657755534,
          7994.929795328776,
          8502.39213017055,
          8441.830290730793,
          8479.31469930013
        ],
        "borderColor": "#4A6FA5",
        "backgroundColor": "#4A6FA5",
        "yAxisID": "y",
        "borderDash": [],
        "pointStyle": "circle"
      },
      {
        "label": "FakeItEasy memory",
        "unit": "b",
        "data": [
          11508,
          11508,
          11508,
          11508,
          11518,
          11518,
          11508,
          11508,
          11508,
          11508,
          11508,
          11508,
          11508,
          11758
        ],
        "borderColor": "#4A6FA5",
        "backgroundColor": "#4A6FA5",
        "yAxisID": "y1",
        "borderDash": [
          5,
          5
        ],
        "pointStyle": "triangle"
      },
      {
        "label": "Imposter time",
        "unit": "ns",
        "data": [
          442.4029050191244,
          481.68347917284285,
          558.3021382013957,
          461.53058325449626,
          539.1291323343913,
          463.2569613774618,
          477.8987444468907,
          438.0488909312657,
          470.94423802693683,
          437.4147892338889,
          485.5060500365037,
          469.82863636016845,
          474.94875839778354,
          457.519069480896
        ],
        "borderColor": "#E84393",
        "backgroundColor": "#E84393",
        "yAxisID": "y",
        "borderDash": [],
        "pointStyle": "circle"
      },
      {
        "label": "Imposter memory",
        "unit": "b",
        "data": [
          3200,
          3200,
          3200,
          3200,
          3200,
          3200,
          3200,
          3200,
          3200,
          3200,
          3200,
          3200,
          3200,
          3200
        ],
        "borderColor": "#E84393",
        "backgroundColor": "#E84393",
        "yAxisID": "y1",
        "borderDash": [
          5,
          5
        ],
        "pointStyle": "triangle"
      },
      {
        "label": "TUnitMocks time",
        "unit": "ns",
        "data": [
          726.1729008601262,
          713.4142534891764,
          884.2634745915731,
          772.0946531295776,
          847.0446955998739,
          826.8433941523234,
          748.0636875788371,
          724.8868615468343,
          751.7523137606107,
          709.4960280198318,
          697.6314438501994,
          723.1458973203387,
          694.557765197754,
          749.8401639302572
        ],
        "borderColor": "#FF8C00",
        "backgroundColor": "#FF8C00",
        "yAxisID": "y",
        "borderDash": [],
        "pointStyle": "circle"
      },
      {
        "label": "TUnitMocks memory",
        "unit": "b",
        "data": [
          2568,
          2568,
          2568,
          2568,
          2568,
          2568,
          2568,
          2568,
          2568,
          2568,
          2568,
          2568,
          2568,
          2568
        ],
        "borderColor": "#FF8C00",
        "backgroundColor": "#FF8C00",
        "yAxisID": "y1",
        "borderDash": [
          5,
          5
        ],
        "pointStyle": "triangle"
      }
    ]
  },
  "Property (N=10)": {
    "commits": [
      {
        "sha": "3eb0d03f222ccc5ac42a2f32e78546cc9c633150",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T13:26:28Z",
        "message": "feat: add baseline benchmark comparison to reporting (#744)"
      },
      {
        "sha": "381840acc2d2a841abbc64369aede5f364acdb1f",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T13:54:51Z",
        "message": "refactor: enhance benchmark table display names (#746)"
      },
      {
        "sha": "ace2d4ac2db5c68606ef27eb394c78f514e5557d",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T15:29:24Z",
        "message": "coverage: add build tests (#747)"
      },
      {
        "sha": "aac7d3511c8ac5e09b9d3ef995fc3b52c70b098a",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T15:52:53Z",
        "message": "perf: optimize generator-emitted setups by skipping dual registration (#741)"
      },
      {
        "sha": "c5047af16fa6992d7704cbfa089919c96091d884",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T16:25:14Z",
        "message": "fix: build error (#749)"
      },
      {
        "sha": "7f5a215d0dcc2c82a88f3fa2bd0aa7666848b9f8",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T16:39:28Z",
        "message": "perf: lazy-allocate per-member fast buffers on first record (#740)"
      },
      {
        "sha": "c28709e4647b9abe55894504efcf4d69941173cb",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T20:01:07Z",
        "message": "feat: bind \u0060Method(default, \u2026)\u0060 to the by-values overload (#750)"
      },
      {
        "sha": "200d0bb9827367185c99c06a7d05d7c9f84701bc",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T20:40:13Z",
        "message": "docs: update migration package description to include NSubstitute support (#751)"
      },
      {
        "sha": "64156009dafaca073bdfea9a5b04562dee1cc050",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-02T06:08:23Z",
        "message": "refactor: move source-generated mock to top of file (#752)"
      },
      {
        "sha": "681a2cfdd105fbed7fdd389922b2aee58bae897f",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-02T06:28:36Z",
        "message": "fix: make source generator snapshot build configuration invariant (#753)"
      },
      {
        "sha": "812a93f05fb7bbaf016d8b6e82a455a4270e591e",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-02T06:44:29Z",
        "message": "refactor: enable nullable reference types in BenchmarkReport (#754)"
      },
      {
        "sha": "a56e91e48833343e983992a69d2fe77b953b8f9c",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-02T20:56:04Z",
        "message": "docs: notify Testably.Site when documentation changes (#755)"
      },
      {
        "sha": "a3038970faba5546782c1657f6f0927ca5fcd930",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-02T22:10:02Z",
        "message": "refactor: update repository references from aweXpect to Testably (#756)"
      },
      {
        "sha": "557bcbc8ab11c6c1c0101656ed73d210511ba24b",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-03T05:56:25Z",
        "message": "fix: casing of sonar organization (#760)"
      }
    ],
    "labels": [
      "3eb0d03f",
      "381840ac",
      "ace2d4ac",
      "aac7d351",
      "c5047af1",
      "7f5a215d",
      "c28709e4",
      "200d0bb9",
      "64156009",
      "681a2cfd",
      "812a93f0",
      "a56e91e4",
      "a3038970",
      "557bcbc8"
    ],
    "datasets": [
      {
        "label": "Mockolate time",
        "unit": "ns",
        "data": [
          1030.674302646092,
          1012.6596020289829,
          1128.9913347880045,
          1063.3139366149903,
          1275.5410342897687,
          1242.9850537618001,
          1162.4108084360757,
          1015.9508698327201,
          1109.775306447347,
          1031.2867749077934,
          1041.6379185994467,
          1054.4572007497152,
          1022.1589904512678,
          1266.7546068338247
        ],
        "borderColor": "#63A2AC",
        "backgroundColor": "#63A2AC",
        "yAxisID": "y",
        "borderDash": [],
        "pointStyle": "circle"
      },
      {
        "label": "Mockolate memory",
        "unit": "b",
        "data": [
          3024,
          3024,
          3024,
          3024,
          3024,
          3024,
          3024,
          3024,
          3024,
          3024,
          3024,
          3024,
          3024,
          3024
        ],
        "borderColor": "#63A2AC",
        "backgroundColor": "#63A2AC",
        "yAxisID": "y1",
        "borderDash": [
          5,
          5
        ],
        "pointStyle": "triangle"
      },
      {
        "label": "Moq time",
        "unit": "ns",
        "data": [
          15850.951649257115,
          18691.180701622598,
          19641.02499171666,
          18716.013082650992,
          17826.103912353516,
          17536.843837483724,
          19197.169135460488,
          19004.925563267298,
          19559.746474202475,
          19420.815059407552,
          18476.456697591148,
          18905.889504568917,
          18810.118432617186,
          18073.78383585612
        ],
        "borderColor": "#A052B0",
        "backgroundColor": "#A052B0",
        "yAxisID": "y",
        "borderDash": [],
        "pointStyle": "circle"
      },
      {
        "label": "Moq memory",
        "unit": "b",
        "data": [
          17441,
          17441,
          18721,
          18721,
          18721,
          18721,
          18833,
          18721,
          18721,
          18721,
          17441,
          18801,
          17441,
          18801
        ],
        "borderColor": "#A052B0",
        "backgroundColor": "#A052B0",
        "yAxisID": "y1",
        "borderDash": [
          5,
          5
        ],
        "pointStyle": "triangle"
      },
      {
        "label": "NSubstitute time",
        "unit": "ns",
        "data": [
          16176.892504882813,
          17353.05517108624,
          18736.99809773763,
          17340.22935180664,
          16760.43368966239,
          16452.38395385742,
          17037.872050694055,
          17109.914276123047,
          17597.28555094401,
          17078.560616048177,
          17004.284210205078,
          16667.667336600167,
          16657.44989013672,
          17377.043214925132
        ],
        "borderColor": "#5E2750",
        "backgroundColor": "#5E2750",
        "yAxisID": "y",
        "borderDash": [],
        "pointStyle": "circle"
      },
      {
        "label": "NSubstitute memory",
        "unit": "b",
        "data": [
          21585,
          21585,
          21585,
          21585,
          21585,
          21585,
          21585,
          21585,
          21585,
          21585,
          21585,
          21585,
          21585,
          21585
        ],
        "borderColor": "#5E2750",
        "backgroundColor": "#5E2750",
        "yAxisID": "y1",
        "borderDash": [
          5,
          5
        ],
        "pointStyle": "triangle"
      },
      {
        "label": "FakeItEasy time",
        "unit": "ns",
        "data": [
          16887.873120117187,
          19718.646718343098,
          21012.187467447915,
          20603.94292776925,
          18810.25871712821,
          18316.52256266276,
          20139.41883951823,
          19596.303834181566,
          20680.881001063757,
          20650.816424560548,
          19685.50981241862,
          20133.02573852539,
          20151.92573038737,
          19220.562368539664
        ],
        "borderColor": "#4A6FA5",
        "backgroundColor": "#4A6FA5",
        "yAxisID": "y",
        "borderDash": [],
        "pointStyle": "circle"
      },
      {
        "label": "FakeItEasy memory",
        "unit": "b",
        "data": [
          31546,
          31546,
          31546,
          31546,
          31543,
          31543,
          31546,
          31546,
          31546,
          31546,
          31546,
          31546,
          31546,
          31783
        ],
        "borderColor": "#4A6FA5",
        "backgroundColor": "#4A6FA5",
        "yAxisID": "y1",
        "borderDash": [
          5,
          5
        ],
        "pointStyle": "triangle"
      },
      {
        "label": "Imposter time",
        "unit": "ns",
        "data": [
          1093.7415985107423,
          1133.7005030768257,
          1298.8523755391438,
          1096.1467549641927,
          1410.6055096944174,
          1243.1338586171469,
          1204.3833024342855,
          1143.184272257487,
          1133.3367366790771,
          1078.9781089782714,
          1123.2838494618734,
          1154.9367290496825,
          1155.1578136171613,
          1283.2917110443116
        ],
        "borderColor": "#E84393",
        "backgroundColor": "#E84393",
        "yAxisID": "y",
        "borderDash": [],
        "pointStyle": "circle"
      },
      {
        "label": "Imposter memory",
        "unit": "b",
        "data": [
          4784,
          4784,
          4784,
          4784,
          4784,
          4784,
          4784,
          4784,
          4784,
          4784,
          4784,
          4784,
          4784,
          4784
        ],
        "borderColor": "#E84393",
        "backgroundColor": "#E84393",
        "yAxisID": "y1",
        "borderDash": [
          5,
          5
        ],
        "pointStyle": "triangle"
      },
      {
        "label": "TUnitMocks time",
        "unit": "ns",
        "data": [
          2232.18229598999,
          2089.452853339059,
          2409.7523846944173,
          2167.225860087077,
          2748.9945467631023,
          2653.0629007975263,
          2234.3687472025554,
          2130.530116762434,
          2206.680022684733,
          2196.3415132250107,
          2160.6665919377256,
          2148.589050547282,
          2269.8034072875976,
          2680.9508595784505
        ],
        "borderColor": "#FF8C00",
        "backgroundColor": "#FF8C00",
        "yAxisID": "y",
        "borderDash": [],
        "pointStyle": "circle"
      },
      {
        "label": "TUnitMocks memory",
        "unit": "b",
        "data": [
          4776,
          4776,
          4776,
          4776,
          4776,
          4776,
          4776,
          4776,
          4776,
          4776,
          4776,
          4776,
          4776,
          4776
        ],
        "borderColor": "#FF8C00",
        "backgroundColor": "#FF8C00",
        "yAxisID": "y1",
        "borderDash": [
          5,
          5
        ],
        "pointStyle": "triangle"
      }
    ]
  },
  "CreateMock": {
    "commits": [
      {
        "sha": "3eb0d03f222ccc5ac42a2f32e78546cc9c633150",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T13:26:28Z",
        "message": "feat: add baseline benchmark comparison to reporting (#744)"
      },
      {
        "sha": "381840acc2d2a841abbc64369aede5f364acdb1f",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T13:54:51Z",
        "message": "refactor: enhance benchmark table display names (#746)"
      },
      {
        "sha": "ace2d4ac2db5c68606ef27eb394c78f514e5557d",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T15:29:24Z",
        "message": "coverage: add build tests (#747)"
      },
      {
        "sha": "aac7d3511c8ac5e09b9d3ef995fc3b52c70b098a",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T15:52:53Z",
        "message": "perf: optimize generator-emitted setups by skipping dual registration (#741)"
      },
      {
        "sha": "c5047af16fa6992d7704cbfa089919c96091d884",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T16:25:14Z",
        "message": "fix: build error (#749)"
      },
      {
        "sha": "7f5a215d0dcc2c82a88f3fa2bd0aa7666848b9f8",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T16:39:28Z",
        "message": "perf: lazy-allocate per-member fast buffers on first record (#740)"
      },
      {
        "sha": "c28709e4647b9abe55894504efcf4d69941173cb",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T20:01:07Z",
        "message": "feat: bind \u0060Method(default, \u2026)\u0060 to the by-values overload (#750)"
      },
      {
        "sha": "200d0bb9827367185c99c06a7d05d7c9f84701bc",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-01T20:40:13Z",
        "message": "docs: update migration package description to include NSubstitute support (#751)"
      },
      {
        "sha": "64156009dafaca073bdfea9a5b04562dee1cc050",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-02T06:08:23Z",
        "message": "refactor: move source-generated mock to top of file (#752)"
      },
      {
        "sha": "681a2cfdd105fbed7fdd389922b2aee58bae897f",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-02T06:28:36Z",
        "message": "fix: make source generator snapshot build configuration invariant (#753)"
      },
      {
        "sha": "812a93f05fb7bbaf016d8b6e82a455a4270e591e",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-02T06:44:29Z",
        "message": "refactor: enable nullable reference types in BenchmarkReport (#754)"
      },
      {
        "sha": "a56e91e48833343e983992a69d2fe77b953b8f9c",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-02T20:56:04Z",
        "message": "docs: notify Testably.Site when documentation changes (#755)"
      },
      {
        "sha": "a3038970faba5546782c1657f6f0927ca5fcd930",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-02T22:10:02Z",
        "message": "refactor: update repository references from aweXpect to Testably (#756)"
      },
      {
        "sha": "557bcbc8ab11c6c1c0101656ed73d210511ba24b",
        "author": "Valentin Breu\u00DF",
        "date": "2026-05-03T05:56:25Z",
        "message": "fix: casing of sonar organization (#760)"
      }
    ],
    "labels": [
      "3eb0d03f",
      "381840ac",
      "ace2d4ac",
      "aac7d351",
      "c5047af1",
      "7f5a215d",
      "c28709e4",
      "200d0bb9",
      "64156009",
      "681a2cfd",
      "812a93f0",
      "a56e91e4",
      "a3038970",
      "557bcbc8"
    ],
    "datasets": [
      {
        "label": "Mockolate time",
        "unit": "ns",
        "data": [
          206.43904205468985,
          206.90384244918823,
          197.4791258573532,
          185.55214584790744,
          214.22546648979187,
          66.70800777276357,
          61.68514164288839,
          66.91532316207886,
          66.1631269534429,
          69.45676867961883,
          65.9090805610021,
          65.7765565101917,
          55.56023137569427,
          64.58949823379517
        ],
        "borderColor": "#63A2AC",
        "backgroundColor": "#63A2AC",
        "yAxisID": "y",
        "borderDash": [],
        "pointStyle": "circle"
      },
      {
        "label": "Mockolate memory",
        "unit": "b",
        "data": [
          1048,
          1048,
          1048,
          1048,
          1048,
          440,
          440,
          440,
          440,
          440,
          440,
          440,
          440,
          440
        ],
        "borderColor": "#63A2AC",
        "backgroundColor": "#63A2AC",
        "yAxisID": "y1",
        "borderDash": [
          5,
          5
        ],
        "pointStyle": "triangle"
      },
      {
        "label": "Imposter time",
        "unit": "ns",
        "data": [
          324.95665920697724,
          291.77834569490875,
          284.32802251407077,
          265.16690225601195,
          283.34178568522134,
          293.77400776545204,
          272.52040631954486,
          302.7673050244649,
          309.7344483693441,
          312.01855414708456,
          283.99031213351657,
          272.7877186536789,
          259.75826791354586,
          282.6282572746277
        ],
        "borderColor": "#E84393",
        "backgroundColor": "#E84393",
        "yAxisID": "y",
        "borderDash": [],
        "pointStyle": "circle"
      },
      {
        "label": "Imposter memory",
        "unit": "b",
        "data": [
          2248,
          2248,
          2248,
          2248,
          2248,
          2248,
          2248,
          2248,
          2248,
          2248,
          2248,
          2248,
          2248,
          2248
        ],
        "borderColor": "#E84393",
        "backgroundColor": "#E84393",
        "yAxisID": "y1",
        "borderDash": [
          5,
          5
        ],
        "pointStyle": "triangle"
      },
      {
        "label": "TUnitMocks time",
        "unit": "ns",
        "data": [
          46.34096393585205,
          45.1492017030716,
          41.01981835705893,
          38.86212600072225,
          42.57269605000814,
          39.9077791755016,
          38.80183372589258,
          42.61832061211268,
          45.07389976297106,
          44.06214502879551,
          43.089406955242154,
          41.959192178646724,
          34.95811413867133,
          41.5128509759903
        ],
        "borderColor": "#FF8C00",
        "backgroundColor": "#FF8C00",
        "yAxisID": "y",
        "borderDash": [],
        "pointStyle": "circle"
      },
      {
        "label": "TUnitMocks memory",
        "unit": "b",
        "data": [
          224,
          224,
          224,
          224,
          224,
          224,
          224,
          224,
          224,
          224,
          224,
          224,
          224,
          224
        ],
        "borderColor": "#FF8C00",
        "backgroundColor": "#FF8C00",
        "yAxisID": "y1",
        "borderDash": [
          5,
          5
        ],
        "pointStyle": "triangle"
      },
      {
        "label": "Moq time",
        "unit": "ns",
        "data": [
          1486.6473520914712,
          1458.401035308838,
          1394.3358195168632,
          1385.3587293624878,
          1459.1881853739421,
          1512.0726731618245,
          1460.5700678144183,
          1412.2708759307861,
          1315.9475856781005,
          1346.923324584961,
          1365.506247584025,
          1353.1228912353515,
          1125.0070772806803,
          1375.9224026019756
        ],
        "borderColor": "#A052B0",
        "backgroundColor": "#A052B0",
        "yAxisID": "y",
        "borderDash": [],
        "pointStyle": "circle"
      },
      {
        "label": "Moq memory",
        "unit": "b",
        "data": [
          2096,
          2096,
          2096,
          2096,
          2096,
          2096,
          2096,
          2096,
          2096,
          2096,
          2096,
          2096,
          2096,
          2096
        ],
        "borderColor": "#A052B0",
        "backgroundColor": "#A052B0",
        "yAxisID": "y1",
        "borderDash": [
          5,
          5
        ],
        "pointStyle": "triangle"
      },
      {
        "label": "NSubstitute time",
        "unit": "ns",
        "data": [
          2113.6252649943035,
          2107.618301645915,
          2031.8937438964845,
          1957.9038787841796,
          2111.4198104858397,
          1973.6907386779785,
          1948.8667559305827,
          2016.3092757004958,
          1890.891881306966,
          1945.1267677307128,
          1846.3399634728064,
          1816.860245337853,
          1635.3277022497994,
          1963.2184542338052
        ],
        "borderColor": "#5E2750",
        "backgroundColor": "#5E2750",
        "yAxisID": "y",
        "borderDash": [],
        "pointStyle": "circle"
      },
      {
        "label": "NSubstitute memory",
        "unit": "b",
        "data": [
          5048,
          5048,
          5048,
          5048,
          5048,
          5048,
          5048,
          5048,
          5048,
          5048,
          5048,
          5048,
          5048,
          5048
        ],
        "borderColor": "#5E2750",
        "backgroundColor": "#5E2750",
        "yAxisID": "y1",
        "borderDash": [
          5,
          5
        ],
        "pointStyle": "triangle"
      },
      {
        "label": "FakeItEasy time",
        "unit": "ns",
        "data": [
          1961.2445017496746,
          1837.986663945516,
          1780.4835116068523,
          1739.6222978591918,
          1850.7287448883058,
          1779.2218757356916,
          1926.685120900472,
          1833.5760401317052,
          1759.6162789662678,
          1735.5245652516683,
          1786.0549332754952,
          1726.3554037412007,
          1712.4860379536947,
          1727.8506146748862
        ],
        "borderColor": "#4A6FA5",
        "backgroundColor": "#4A6FA5",
        "yAxisID": "y",
        "borderDash": [],
        "pointStyle": "circle"
      },
      {
        "label": "FakeItEasy memory",
        "unit": "b",
        "data": [
          2763,
          2763,
          2763,
          2763,
          2763,
          2763,
          2763,
          2763,
          2772,
          2772,
          2763,
          2763,
          2763,
          2763
        ],
        "borderColor": "#4A6FA5",
        "backgroundColor": "#4A6FA5",
        "yAxisID": "y1",
        "borderDash": [
          5,
          5
        ],
        "pointStyle": "triangle"
      }
    ]
  }
}