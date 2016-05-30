using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Chuye.Persistent.Mongo {
    public class MongoRepository<TEntry, TKey> : Repository<TEntry> where TEntry : IMongoAggregate<TKey> {
        private readonly MongoRepositoryContext _context = null;
        private readonly IMongoRepositoryMapper _mapper;

        public MongoRepositoryContext MongoRepositoryContext {
            get { return _context; }
        }

        public MongoRepository(IRepositoryContext context)
            : this(context, new MongoRepositoryMapper()) {
        }

        public MongoRepository(IRepositoryContext context, IMongoRepositoryMapper mapper)
            : base(context) {
            _context = context as MongoRepositoryContext;
            if (_context == null) {
                throw new ArgumentOutOfRangeException("context",
                    "Expect MongoRepositoryContext but provided " + context.GetType().FullName);
            }
            _mapper = mapper;
        }

        public override IQueryable<TEntry> All {
            get {
                var collectionName = _mapper.Map<TEntry>();
                var collection = _context.Database.GetCollection<TEntry>(collectionName);
                return collection.AsQueryable();
            }
        }

        public override TReutrn Fetch<TReutrn>(Func<IQueryable<TEntry>, TReutrn> query) {
            return query(All);
        }

        public override TEntry Retrive(Object id) {
            //Builders<TEntry>.Filter.Eq(e => e.Id, id)
            //return docs.Find(new FilterDefinitionBuilder<TEntry>().Eq(e => e.Id, id), new FindOptions()).Limit(1).FirstOrDefault();

            var collection = GetCollection();
            return collection.Find(new FilterDefinitionBuilder<TEntry>().Eq(e => e.Id, id), new FindOptions()).Limit(1).FirstOrDefault();
            //var queryDocument = new BsonDocument("_id", BsonValue.Create(id));
            //return collection.Find(queryDocument).FirstOrDefault();
        }

        private IMongoCollection<TEntry> GetCollection() {
            var collectionName = _mapper.Map<TEntry>();
            var collection = _context.Database.GetCollection<TEntry>(collectionName);
            return collection;
        }

        public override IEnumerable<TEntry> Retrive<TMember>(String field, params TMember[] keys) {
            var collection = GetCollection();
            return collection.Find(new FilterDefinitionBuilder<TEntry>()
                .In(field,keys.Select(k => BsonValue.Create(k)))).ToList();
        }

        public override IEnumerable<TEntry> Retrive<TMember>(Expression<Func<TEntry, TMember>> selector, params TMember[] keys) {
            var collection = GetCollection();
            return collection.Find(new FilterDefinitionBuilder<TEntry>()
                .In(selector, keys)).ToList();
        }

        public override void Create(TEntry entry) {
            var collection = GetCollection();
            collection.InsertOne(entry);
        }        

        public override void Update(TEntry entry) {
            var collection = GetCollection();
            collection.FindOneAndReplace(new FilterDefinitionBuilder<TEntry>().Eq(r => r.Id, entry.Id), entry);
        }

        public void Update<TMember>(TKey id, Expression<Func<TEntry, TMember>> memberExpression, TMember value) {
            var collection = GetCollection();
            collection.FindOneAndUpdate(new FilterDefinitionBuilder<TEntry>().Eq(r => r.Id, id),
                new UpdateDefinitionBuilder<TEntry>().Set(memberExpression, value),
                new FindOneAndUpdateOptions<TEntry, TEntry>() { IsUpsert = true });
        }


        public override void Save(TEntry entry) {
            var collection = GetCollection();
            collection.FindOneAndReplace(new FilterDefinitionBuilder<TEntry>().Eq(r => r.Id, entry.Id),
                entry, new FindOneAndReplaceOptions<TEntry, TEntry>() { IsUpsert = true });
        }
        public override void Delete(TEntry entry) {
            var collection = GetCollection();
            collection.DeleteOne(new FilterDefinitionBuilder<TEntry>().Eq(r => r.Id, entry.Id));
        }

        public override bool Any(params Expression<Func<TEntry, bool>>[] predicates) {
            IQueryable<TEntry> query = All;
            foreach (var predicate in predicates) {
                query = query.Where(predicate);
            }
            return query.Select(r => r.Id).Any();
        }
    }

}
