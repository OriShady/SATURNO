using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace NoSqlU.Data;

public sealed class MongoCatalog<T, TId>
{
    private readonly IMongoCollection<T> _collection;
    private readonly Func<string, Task<int>>? _nextId;
    private readonly BsonMemberMap _id = BsonClassMap.LookupClassMap(typeof(T)).IdMemberMap;

    internal MongoCatalog(IMongoCollection<T> collection, Func<string, Task<int>>? nextId = null)
    {
        _collection = collection;
        _nextId = nextId;
    }

    public IQueryable<T> Query => _collection.AsQueryable();

    private FilterDefinition<T> ById(TId id) =>
        new BsonDocument("_id", id is char code ? new BsonString(code.ToString()) : BsonValue.Create(id));

    public async Task<T?> FindAsync(TId id) =>
        await _collection.Find(ById(id)).FirstOrDefaultAsync();

    public async Task InsertAsync(T entity)
    {
        if (_nextId is not null)
            _id.Setter(entity!, await _nextId(_collection.CollectionNamespace.CollectionName));
        await _collection.InsertOneAsync(entity);
    }

    public Task ReplaceAsync(T entity) =>
        _collection.ReplaceOneAsync(ById((TId)_id.Getter(entity!)), entity);

    public Task DeleteAsync(TId id) => _collection.DeleteOneAsync(ById(id));
}
