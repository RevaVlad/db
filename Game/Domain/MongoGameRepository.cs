using System;
using System.Collections.Generic;
using MongoDB.Driver;

namespace Game.Domain
{
    // TODO Сделать по аналогии с MongoUserRepository
    public class MongoGameRepository : IGameRepository
    {
        private readonly IMongoCollection<GameEntity> gamesCollection;
        public const string CollectionName = "games";

        public MongoGameRepository(IMongoDatabase db)
        {
            gamesCollection = db.GetCollection<GameEntity>(CollectionName);
        }

        public GameEntity Insert(GameEntity game)
        {
            gamesCollection.InsertOne(game);
            return game;
        }

        public GameEntity FindById(Guid gameId)
        {
            var filter = Builders<GameEntity>.Filter.Eq(g => g.Id, gameId);
            return gamesCollection.Find(filter).FirstOrDefault();
        }

        public void Update(GameEntity game)
        {
            var filter = Builders<GameEntity>.Filter.Eq(g => g.Id, game.Id);
            gamesCollection.ReplaceOne(filter, game);
        }

        // Возвращает не более чем limit игр со статусом GameStatus.WaitingToStart
        public IList<GameEntity> FindWaitingToStart(int limit)
        {
            var filter = Builders<GameEntity>.Filter.Eq(x => x.Status, GameStatus.WaitingToStart);
            return gamesCollection.Find(filter).Limit(limit).ToList();
        }

        // Обновляет игру, если она находится в статусе GameStatus.WaitingToStart
        public bool TryUpdateWaitingToStart(GameEntity game)
        {
            var filter = Builders<GameEntity>.Filter.And(
                Builders<GameEntity>.Filter.Eq(g => g.Id, game.Id),
                Builders<GameEntity>.Filter.Eq(g => g.Status, GameStatus.WaitingToStart));

            var replaceOneResult = gamesCollection.ReplaceOne(filter, game);
            
            return replaceOneResult.IsAcknowledged && replaceOneResult.ModifiedCount == 1;
        }
    }
}