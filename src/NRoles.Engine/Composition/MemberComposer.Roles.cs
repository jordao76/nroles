using System.Collections.Generic;

namespace NRoles.Engine {
  
  sealed partial class MemberComposer : IMessageContainer, Does<RMessageContainer> {

    public IEnumerable<Message> Messages { [Placeholder] get { throw Away.Code; } }
    [Placeholder] public void AddMessage(Message message) { throw Away.Code; }

  }

}
