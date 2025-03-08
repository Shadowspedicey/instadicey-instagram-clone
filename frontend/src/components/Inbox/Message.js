import { Link } from "react-router-dom";

const Message = ({messageInfo, noPhoto, isSent}) =>
{
	return(
		<div className={`message ${isSent ? "sent" : ""}`}>
			{ noPhoto
				? <span className="dummy" style={{width: 32, height: 32}}></span>
				: <Link to={`/${messageInfo.user.username}`} className="profile-pic"><img src={messageInfo.user.profilePic} alt={`${messageInfo.user.username}'s profile pic`}></img></Link>
			}
			<span className="text outlined round">{messageInfo.message}</span>
		</div>
	);
};

export default Message;
