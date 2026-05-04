using TankDestroyer.API;

namespace Nico.Bot;

[Bot("Nico Destroyer", "Nico", "FF0000")]
public class NicoBot : IPlayerBot
{
   private readonly Random _random = new();

   public void DoTurn(ITurnContext turnContext)
   {
       if (TryDodgeBullet(turnContext))
           return;

       var myTank = turnContext.Tank;

       var enemy = turnContext.GetTanks()
           .Where(t => !t.Destroyed && t.OwnerId != myTank.OwnerId)
           .OrderBy(t => Distance(myTank, t))
           .FirstOrDefault();

       if (enemy is not null)
       {
           turnContext.RotateTurret(GetTurretDirection(myTank, enemy));
           turnContext.Fire();
       }

       MoveToRandomSafeTile(turnContext);
   }

   private bool TryDodgeBullet(ITurnContext turnContext)
   {
       var myTank = turnContext.Tank;

       var dangerousBullet = turnContext.GetBullets()
           .FirstOrDefault(b =>
               Math.Abs(b.X - myTank.X) <= 2 &&
               Math.Abs(b.Y - myTank.Y) <= 2);

       if (dangerousBullet is null)
           return false;

       MoveToRandomSafeTile(turnContext);
       return true;
   }

   private void MoveToRandomSafeTile(ITurnContext turnContext)
   {
       var directions = Enum.GetValues<Direction>()
           .OrderBy(_ => _random.Next())
           .ToList();

       foreach (var direction in directions)
       {
           var nextX = turnContext.Tank.X;
           var nextY = turnContext.Tank.Y;

           switch (direction)
           {
               case Direction.North:
                   nextY += 1;
                   break;

               case Direction.South:
                   nextY -= 1;
                   break;

               case Direction.East:
                   nextX -= 1;
                   break;

               case Direction.West:
                   nextX += 1;
                   break;
           }

           if (IsSafeTile(turnContext, nextY, nextX))
           {
               turnContext.MoveTank(direction);
               return;
           }
       }
   }

   private static bool IsSafeTile(ITurnContext turnContext, int y, int x)
   {
       if (x < 0 || y < 0) return false;
       if (x >= turnContext.GetMapWidth()) return false;
       if (y >= turnContext.GetMapHeight()) return false;

       var tile = turnContext.GetTile(y, x);

       return tile.TileType != TileType.Water;
   }

   private static int Distance(ITank a, ITank b)
   {
       return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
   }

   private static TurretDirection GetTurretDirection(ITank myTank, ITank enemy)
   {
       var dx = enemy.X - myTank.X;
       var dy = enemy.Y - myTank.Y;

       if (dx > 0 && dy > 0) return TurretDirection.NorthWest;
       if (dx > 0 && dy < 0) return TurretDirection.SouthWest;
       if (dx < 0 && dy > 0) return TurretDirection.NorthEast;
       if (dx < 0 && dy < 0) return TurretDirection.SouthEast;

       if (dx > 0) return TurretDirection.West;
       if (dx < 0) return TurretDirection.East;
       if (dy > 0) return TurretDirection.North;

       return TurretDirection.South;
   }
}
