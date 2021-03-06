/*************************************************************************
 *
 *   file		: RestoreManaPercent.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-17 17:38:11 +0100 (sø, 17 jan 2010) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1198 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using WCell.Constants;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Effects
{
	public class RestoreManaPercentEffectHandler : SpellEffectHandler
	{
		public RestoreManaPercentEffectHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}

		protected override void Apply(WorldObject target)
		{
			if (((Unit)target).PowerType == PowerType.Mana)
			{
				var manavalue = ((((Unit)target).MaxPower * Effect.MiscValue) / 100);
				((Unit)target).Energize(m_cast.Caster, manavalue, Effect);
			}
		}
		public override ObjectTypes TargetType
		{
			get { return ObjectTypes.Unit; }
		}
	}
}