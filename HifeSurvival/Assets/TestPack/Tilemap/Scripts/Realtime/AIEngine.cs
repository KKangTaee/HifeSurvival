using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIEngine
{
    public interface IUpdate<T> where T : ServerObject
    {
        void Update(T self,  double deltaTime);
    }

    public interface IState<T> where T : ServerObject
    {
        void Enter(T self, ServerObject other);
        
        void Exit(T self,  ServerObject other);
    }

    public class IdleState : IState<ServerMonster>
    {
        public void Enter(ServerMonster self, ServerObject other)
        {
        
        }

        public void Exit(ServerMonster self, ServerObject other)
        {
         
        }
    }

    public class AttackState : IState<ServerMonster>, IUpdate<ServerMonster>
    {
        public void Enter(ServerMonster self, ServerObject other)
        {

        }

        public void Exit(ServerMonster self, ServerObject other)
        {

        }

        public void Update(ServerMonster self,  double deltaTime)
        {

        }
    }

    public class FollowState : IState<ServerMonster>, IUpdate<ServerMonster>
    {
        public void Enter(ServerMonster self, ServerObject other)
        {
          
        }

        public void Exit(ServerMonster self, ServerObject other)
        {
           
        }

        public void Update(ServerMonster self, double deltaTime)
        {

        }
    }
}
