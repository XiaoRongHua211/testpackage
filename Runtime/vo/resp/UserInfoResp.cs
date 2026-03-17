
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace HumanSDK.VO.Req
{
  public class UserInfoResp
  {
    public string uuid { get; set; } = string.Empty;
    public string name { get; set; } = string.Empty;
    public string email { get; set; } = string.Empty;

    public string phone { get; set; } = string.Empty;
    public string source { get; set; } = string.Empty;
    public string state { get; set; } = string.Empty;

    public BodyCharacter bodyCharacter { get; set; } = new BodyCharacter();
  }

  public class BodyCharacter
  {
    public CharacterValue height { get; set; } = new CharacterValue();
    public CharacterValue armSpan { get; set; } = new CharacterValue();
    public CharacterValue legLength { get; set; } = new CharacterValue();

  }

  public class CharacterValue
  {
    public float? value { get; set; }
    public string? unit { get; set; }
  }
}
