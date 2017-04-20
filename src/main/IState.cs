using System.Collections.Generic;

namespace Learn 
{ 
    public interface IState
    {
        List<IState> getSuccessors();
    }
}