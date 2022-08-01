using System.Collections.Generic;

namespace PMB.Browsers.Common;

public record StakeCoefficientResult(bool IsStaked, decimal Coefficient);

public record PriceInfo(string Designation, double Price);

public record StakeUniversal(string Type,List<PriceInfo> Prices,string Key);