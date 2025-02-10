﻿using RabbitQuestAPI.Application.DTO;
using RabbitQuestAPI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace RabbitQuestAPI.Application.Interfaces
{
    public interface IUserRepository : IRepositoryBase<User>
    {
        
    }
}
