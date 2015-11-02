﻿// This file is Part of CalDavSynchronizer (http://outlookcaldavsynchronizer.sourceforge.net/)
// Copyright (c) 2015 Gerhard Zehetbauer 
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenSync.EntityRepositories;

namespace GenSync.UnitTests.Synchronization.Stubs
{
  internal class TestRepository : IEntityRepository<string, Identifier, int>
  {
    private readonly string _idPrefix;

    public readonly Dictionary<Identifier, Tuple<int, string>> EntityVersionAndContentById = new Dictionary<Identifier, Tuple<int, string>> (IdentifierEqualityComparer.Instance);
    private int _nextId = 1;


    public int Count
    {
      get { return EntityVersionAndContentById.Count; }
    }

    public TestRepository (string idPrefix)
    {
      _idPrefix = idPrefix;
    }

    public Task<IReadOnlyList<EntityVersion<Identifier, int>>> GetVersions (ICollection<Identifier> ids)
    {
      return Task.FromResult<IReadOnlyList<EntityVersion<Identifier, int>>> (
          ids.Select (id => EntityVersion.Create (id, EntityVersionAndContentById[id].Item1)).ToList());
    }

    public Task<IReadOnlyList<EntityVersion<Identifier, int>>> GetVersions ()
    {
      return Task.FromResult<IReadOnlyList<EntityVersion<Identifier, int>>> (
          EntityVersionAndContentById.Select (kv => EntityVersion.Create (kv.Key, kv.Value.Item1)).ToList());
    }

    public Task<IReadOnlyList<EntityWithId<Identifier, string>>> Get (ICollection<Identifier> ids)
    {
      return Task.FromResult<IReadOnlyList<EntityWithId<Identifier, string>>> (
          ids.Select (id => EntityWithId.Create (id, EntityVersionAndContentById[id].Item2)).ToArray());
    }

    public void Cleanup (IReadOnlyDictionary<Identifier, string> entities)
    {
    }

    public Task Delete (Identifier entityId)
    {
      EntityVersionAndContentById.Remove (entityId);
      return Task.FromResult (0);
    }

    public Task<EntityVersion<Identifier, int>> Update (Identifier entityId, string entityToUpdate, Func<string, string> entityModifier)
    {
      var kv = EntityVersionAndContentById[entityId];
      EntityVersionAndContentById.Remove (entityId);

      var newValue = entityModifier (kv.Item2);
      var newVersion = kv.Item1 + 1;

      var newEntityId = entityId.Value + "u";

      EntityVersionAndContentById[newEntityId] = Tuple.Create (newVersion, newValue);
      return Task.FromResult (new EntityVersion<Identifier, int> (newEntityId, newVersion));
    }

    public EntityVersion<Identifier, int> UpdateWithoutIdChange (Identifier entityId, Func<string, string> entityModifier)
    {
      var kv = EntityVersionAndContentById[entityId];

      var newValue = entityModifier (kv.Item2);
      var newVersion = kv.Item1 + 1;

      EntityVersionAndContentById[entityId] = Tuple.Create (newVersion, newValue);
      return new EntityVersion<Identifier, int> (entityId, newVersion);
    }


    public Task<EntityVersion<Identifier, int>> Create (Func<string, string> entityInitializer)
    {
      var newValue = entityInitializer (string.Empty);
      var entityId = _idPrefix + _nextId++;
      EntityVersionAndContentById[entityId] = Tuple.Create (0, newValue);
      return Task.FromResult (new EntityVersion<Identifier, int> (entityId, 0));
    }

    public Tuple<int, string> this [string id]
    {
      get { return EntityVersionAndContentById[id]; }
    }
  }
}