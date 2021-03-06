/*************************************************************************
 *
 *   file		: TalentHandler.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-06-08 00:55:09 +0800 (Sun, 08 Jun 2008) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 458 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using WCell.Constants;
using WCell.Constants.Talents;
using WCell.Core.DBC;
using WCell.Core.Initialization;
using WCell.Util;
using WCell.Util.Variables;

namespace WCell.RealmServer.Talents
{
	public static partial class TalentMgr
	{
		/// <summary>
		/// Maximum amount of TalentTrees per class (hardcoded in client)
        /// for some reason pet "Cunning" is on tab 4
		/// </summary>
        public const int MaxTabCount = 5;

	    public static readonly int[] TalentResetPriceTiers = {1, 5, 10, 15, 20, 25, 30, 35, 40, 45, 50};

		internal const int MaxTalentRowCount = 20;
		internal const int MaxTalentColCount = 8;

		[NotVariable]
		public static TalentTree[] TalentTrees = new TalentTree[(int)TalentTreeId.End];

		[NotVariable]
		public static TalentTree[][] TreesByClass = new TalentTree[12][];

		[NotVariable]
		public static TalentEntry[] Entries = new TalentEntry[1900];

		/// <summary>
		/// Returns all Trees of the given class
		/// </summary>
		public static TalentTree[] GetTrees(ClassId clss)
		{
			return TreesByClass[(uint)clss];
		}

		/// <summary>
		/// Returns the requested TalentTree
		/// </summary>
		public static TalentTree GetTree(TalentTreeId treeId)
		{
			return TalentTrees[(uint)treeId];
		}

		/// <summary>
		/// Returns the requested TalentEntry
		/// </summary>
		public static TalentEntry GetEntry(TalentId talentId)
		{
			return Entries[(uint)talentId];
		}

		/// <summary>
		/// Depends on SpellHandler
		/// </summary>
		internal static void Initialize()
		{
			var treeReader = new MappedDBCReader<TalentTree, TalentTreeConverter>(
				RealmServerConfiguration.GetDBCFile("TalentTab.dbc"));

			foreach (var tree in treeReader.Entries.Values)
			{
				ArrayUtil.Set(ref TalentTrees, (uint)tree.Id, tree);

				var trees = TreesByClass[(uint)tree.Class];
				if (trees == null)
				{
					TreesByClass[(uint)tree.Class] = trees = new TalentTree[MaxTabCount];
				}

				trees[tree.TabIndex] = tree;
			}


			var talentReader = new ListDBCReader<TalentEntry, TalentConverter>(
				RealmServerConfiguration.GetDBCFile("Talent.dbc"));

			foreach (var talent in talentReader.EntryList)
			{
				ArrayUtil.Set(ref Entries, (uint)talent.Id, talent);
				talent.Tree.Talents.Add(talent);

				var talentRow = talent.Tree.TalentTable[talent.Row];
				if (talentRow == null)
				{
					talent.Tree.TalentTable[talent.Row] = talentRow = new TalentEntry[MaxTalentColCount];
				}
				talentRow[talent.Col] = talent;

				foreach (var spell in talent.Spells)
				{
					if (spell != null)
					{
						//if (spell.Talent != null)
						//{
						//    log.Warn("Spell {0} has Talents: {1} + {2}", spell, spell.Talent, talent);
						//}
						spell.Talent = talent;
						spell.ClassId = talent.Tree.Class;
					}
				}
			}

			// calculate total count of Ranks per Tree and the index of each Talent
			foreach (var tree in treeReader.Entries.Values)
			{
				var rankCount = 0;
				foreach (var talent in tree.Talents)
				{
					rankCount += talent.MaxRank;
				}
				tree.TotalRankCount = rankCount;

				ArrayUtil.Prune(ref tree.TalentTable);

				uint talentIndex = 0;
				for (var rowNum = 0; rowNum < tree.TalentTable.Length; rowNum++)
				{
					if (tree.TalentTable[rowNum] != null)
					{
						ArrayUtil.Prune(ref tree.TalentTable[rowNum]);

						var row = tree.TalentTable[rowNum];
						for (var colNum = 0; colNum < row.Length; colNum++)
						{
							var talent = row[colNum];
							if (talent != null)
							{
								talent.Index = talentIndex;
								talentIndex += (uint)talent.MaxRank;
							}
						}
					}
				}
			}
		} // end initialize
	}
}
