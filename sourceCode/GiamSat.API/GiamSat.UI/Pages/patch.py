import re
with open(e:\\Project\\OVEN\\sourceCode\\GiamSat.API\\GiamSat.UI\\Pages\\RevoReport.razor.cs, r, encoding=utf-8) as f: content = f.read()
content = content.replace(private List<FT09_RevoDatalog> _rawData = new();, private int _stepCount;
 private int _shaftCount;
 private int _hourCount;
 private int _scopeRecordCount;)
with open(e:\\Project\\OVEN\\sourceCode\\GiamSat.API\\GiamSat.UI\\Pages\\RevoReport.razor.cs, w, encoding=utf-8) as f: f.write(content)
