import { useRef, useState } from "react";
import { Link } from "react-router-dom";
import { useDispatch, useSelector } from "react-redux";
import { backend } from "../../config.js";
import { logOut } from "../../helpers.js";
import LoadingPage from "../LoadingPage";
import { setSnackbar } from "../../state/actions/snackbar";
import Loading from "../../assets/misc/loading.jpg";
import { useHistory } from "react-router-dom/cjs/react-router-dom.min";

const ChangePassword = () =>
{
	const dispatch = useDispatch();
	const history = useHistory();
	const currentUser = useSelector(state => state.currentUser);
	const [isInfoValid, setIsInfoValid] = useState(null);
	const [isLoading, setIsLoading] = useState(null);

	const oldPasswordRef = useRef();
	const newPasswordRef = useRef();
	const confirmPasswordRef = useRef();

	const handleOnChange = () =>
		oldPasswordRef.current.value.length > 0
		&& newPasswordRef.current.value.length > 0
		&& confirmPasswordRef.current.value.length > 0
			?	setIsInfoValid(true)
			: setIsInfoValid(false);

	const handleSubmit = async e =>
	{
		e.preventDefault();
		if (!isInfoValid) return;
		
		try
		{
			setIsLoading(true);
			const oldPassword = oldPasswordRef.current.value;
			const newPassword = newPasswordRef.current.value;
			const confirmPassword = confirmPasswordRef.current.value;
			if (newPassword !== confirmPassword) throw new Error("Password don't match");
			if (newPassword.length < 6) throw new Error("Password too short");

			const result = await fetch(`${backend}/auth/change-password`, {
				method: "POST",
				headers: {
					Authorization: `Bearer ${localStorage.token}`,
					"Content-Type": "application/json"
				},
				body: JSON.stringify({
					currentPassword: oldPassword,
					newPassword: newPassword
				})
			});
			if (result.status === 401)
				return logOut(dispatch, history);
			if (!result.ok) {
				const resultJSON = await result.json();
				throw new Error(resultJSON.detail);
			}
			dispatch(setSnackbar("Password updated successfully.", "success"));
		} catch(err)
		{
			dispatch(setSnackbar(err.message ?? "Oops, please try again later.", "error"));
		}
		oldPasswordRef.current.value = "";
		newPasswordRef.current.value = "";
		confirmPasswordRef.current.value = "";
		setIsLoading(false);
	};

	if (!currentUser) return <LoadingPage/>;
	return(
		<div className="account-element change-password">
			<div className="element profile-pic-container">
				<div className="img-container outlined left">
					<img src={currentUser.profilePic} alt={`${currentUser.username}'s profile pic'`}></img>
				</div>
				<div className="right">
					{currentUser.username}
				</div>
			</div>
			<form onSubmit={handleSubmit}>
				<div className="element">
					<div className="left">
						<label htmlFor="old-password">Old Password</label>
					</div>
					<div className="right">
						<input type="password" id="old-password" className="outlined" ref={oldPasswordRef} onChange={handleOnChange}></input>
					</div>
				</div>
				<div className="element">
					<div className="left">
						<label htmlFor="new-password">New Password</label>
					</div>
					<div className="right">
						<input type="password" id="new-password" className="outlined" ref={newPasswordRef} onChange={handleOnChange}></input>
					</div>
				</div>
				<div className="element">
					<div className="left">
						<label htmlFor="confirm-password">Confirm New Password</label>
					</div>
					<div className="right">
						<input type="password" id="confirm-password" className="outlined" ref={confirmPasswordRef} onChange={handleOnChange}></input>
					</div>
				</div>
				<div className="element">
					<div className="left"></div>
					{
						isLoading
							? <button className="loading"><div><img src={Loading} alt="loading"></img></div></button>
							: <button className={`submit ${isInfoValid ? null : "disabled"}`}>Change Password</button>
					}
				</div>
				<div className="element">
					<div className="left"></div>
					<Link to="/accounts/password/reset" className="forgot-password">Forgot Password?</Link>
				</div>
			</form>
		</div>
	);
};

export default ChangePassword;
