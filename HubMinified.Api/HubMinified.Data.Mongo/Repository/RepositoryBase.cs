using AutoMapper;
using AutoMapper.Extensions.ExpressionMapping;
using HubMinified.Data.Mongo.Context;
using HubMinified.Data.Mongo.Entity;
using HubMinified.Data.Mongo.Mapping;
using HubMinified.Domain.Interfaces.Mongo;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


namespace HubMinified.Data.Mongo.Repository
{
    public class RepositoryBase<MODEL, ENTITY> : IRepositoryBase<MODEL>
         where MODEL : class // model
         where ENTITY : EntityBase // entity
    {
        private string Collection;
        protected IMongoCollection<ENTITY> _collection;
        protected readonly MongoContext _context;
        protected readonly IMapper _mapper;

        public RepositoryBase(MongoContext context)
        {
            _context = context;
            Collection = typeof(ENTITY).Name.ToLower();
            _collection = _context.MongoDb.GetCollection<ENTITY>(Collection);
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddExpressionMapping();
                cfg.AddProfile(typeof(MappingProfile));
            });

            _mapper = config.CreateMapper();
        }

        public RepositoryBase(MongoContext context, string collection)
        {
            _context = context;
            Collection = collection;
            _collection = _context.MongoDb.GetCollection<ENTITY>(Collection);
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddExpressionMapping();
                cfg.AddProfile(typeof(MappingProfile));
            });

            _mapper = config.CreateMapper();
        }


        public async Task<int> Save(MODEL model)
        {
            string modelId = ObjectId.GenerateNewId().ToString();
            PropertyInfo id = typeof(MODEL).GetProperties().FirstOrDefault(e => e.Name == "Id")!;
            id.SetValue(model, modelId);

            ENTITY entity = _mapper.Map<ENTITY>(model);
            await _collection.InsertOneAsync(entity);

            return 1;
        }

        public async Task Update(MODEL model, string id)
        {

            ENTITY entity = _mapper.Map<ENTITY>(model);
            entity.UpdatedDate = DateTime.Now;
            await _collection.ReplaceOneAsync(Builders<ENTITY>.Filter.Eq("_id", new ObjectId(id)), entity);

        }

        public async Task Delete(string id)
        {
            await _collection.DeleteOneAsync(Builders<ENTITY>.Filter.Eq("_id", new ObjectId(id)));

        }

        public async Task<long> Count(Expression<Func<MODEL, bool>> where)
        {
            var whereExp = _mapper.Map<Expression<Func<ENTITY, bool>>>(where);

            var n = await _collection.CountDocumentsAsync(whereExp);

            return n;
        }

        public async Task<List<MODEL>> Get(Expression<Func<MODEL, bool>> where, int skip, int take, Expression<Func<MODEL, object>> sort, bool asc = false)
        {

            var whereExp = _mapper.Map<Expression<Func<ENTITY, bool>>>(where);
            var sortExp = _mapper.Map<Expression<Func<ENTITY, object>>>(sort);

            var filtered = _collection.Find(whereExp);

            IOrderedFindFluent<ENTITY, ENTITY> sorted;
            if (asc)
            {
                sorted = filtered.SortBy(sortExp);
            }
            else
            {
                sorted = filtered.SortByDescending(sortExp);
            }

            List<ENTITY> entityList = await sorted.Skip(skip).Limit(take).ToListAsync();

            return _mapper.Map<List<MODEL>>(entityList);
        }

        public async Task<List<MODEL>> Get(Expression<Func<MODEL, bool>> where)
        {

            var whereExp = _mapper.Map<Expression<Func<ENTITY, bool>>>(where);


            List<ENTITY> entityList = await _collection.Find(whereExp).ToListAsync();


            return _mapper.Map<List<MODEL>>(entityList);
        }


        public async Task<MODEL> Find(Expression<Func<MODEL, bool>> where)
        {

            var whereExp = _mapper.Map<Expression<Func<ENTITY, bool>>>(where);


            var filtered = _collection.Find(whereExp);



            ENTITY entity = await filtered.FirstOrDefaultAsync();

            return _mapper.Map<MODEL>(entity);
        }

    }
}