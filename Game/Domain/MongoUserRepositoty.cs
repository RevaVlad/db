using System;
using System.Linq;
using MongoDB.Driver;

namespace Game.Domain
{
    public class MongoUserRepository : IUserRepository
    {
        private readonly IMongoCollection<UserEntity> userCollection;
        public const string CollectionName = "users";

        public MongoUserRepository(IMongoDatabase database)
        {
            userCollection = database.GetCollection<UserEntity>(CollectionName);

            var keys = Builders<UserEntity>.IndexKeys.Ascending(x => x.Login);
            var indexOptions = new CreateIndexOptions { Unique = true };
            userCollection.Indexes.CreateOne(new CreateIndexModel<UserEntity>(keys, indexOptions));
        }

        public UserEntity Insert(UserEntity user)
        {
            userCollection.InsertOne(user);
            return user;
        }

        public UserEntity FindById(Guid id)
        {
            return userCollection.Find(u => u.Id == id).FirstOrDefault();
        }

        public UserEntity GetOrCreateByLogin(string login)
        {
            var filter = Builders<UserEntity>.Filter.Eq(x => x.Login, login);
            
            var updateDefinition = Builders<UserEntity>.Update
                .SetOnInsert(x => x.Id, Guid.NewGuid())
                .SetOnInsert(x => x.Login, login)
                .SetOnInsert(x => x.FirstName, string.Empty)
                .SetOnInsert(x => x.LastName, string.Empty)
                .SetOnInsert(x => x.CurrentGameId, null)
                .SetOnInsert(x => x.GamesPlayed, 0);

            var findOneAndUpdateOptions = new FindOneAndUpdateOptions<UserEntity, UserEntity>
            {
                IsUpsert = true,
                ReturnDocument = ReturnDocument.After
            };
            
            return userCollection.FindOneAndUpdate(filter, updateDefinition, findOneAndUpdateOptions);
        }

        public void Update(UserEntity user)
        {
            userCollection.ReplaceOne(u => u.Id == user.Id, user);
        }

        public void Delete(Guid id)
        {
            userCollection.DeleteOne(u => u.Id == id);
        }

        // Для вывода списка всех пользователей (упорядоченных по логину)
        // страницы нумеруются с единицы
        public PageList<UserEntity> GetPage(int pageNumber, int pageSize)
        {
            //TODO: Тебе понадобятся SortBy, Skip и Limit
            var totalCount = userCollection.CountDocuments(x => true);
            var pageEntities = userCollection
                .Find(x => true)
                .Sort(Builders<UserEntity>.Sort.Ascending(u => u.Login))
                .Skip(pageSize * (pageNumber - 1))
                .Limit(pageSize)
                .ToList();
            
            return new PageList<UserEntity>(pageEntities, totalCount, pageNumber, pageSize);
        }

        // Не нужно реализовывать этот метод
        public void UpdateOrInsert(UserEntity user, out bool isInserted)
        {
            throw new NotImplementedException();
        }
    }
}