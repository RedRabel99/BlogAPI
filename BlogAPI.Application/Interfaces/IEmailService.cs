using System;
using System.Collections.Generic;
using System.Text;

namespace BlogAPI.Application.Interfaces;

public interface IEmailService 
{
    Task Send();
}
