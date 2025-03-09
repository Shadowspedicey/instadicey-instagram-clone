/* eslint-disable react-hooks/exhaustive-deps */
import { useEffect, useRef, useState } from "react";
import { useDispatch } from "react-redux";
import { startLoading, stopLoading } from "../../state/actions/isLoading";
import Logo from "../../assets/logo.png";
import greenCheckmark from "../../assets/misc/green-checkmark.png";
import redX from "../../assets/misc/red-x.png";
import { backend } from "../../config";
import { useHistory, useLocation } from "react-router-dom/cjs/react-router-dom.min";
import { logOut, refreshOrLogout } from "../../helpers";

const AccountVerification = () =>
{
	const dispatch = useDispatch();
	const history = useHistory();
	const {search} = useLocation();
	const params = new URLSearchParams(search);
	const mode = params.get("mode");
	const email = params.get("user");
	const token = params.get("token");
	const [errorMsg, setErrorMsg] = useState("");
	const [status, setStatus] = useState({});

	const passwordRef = useRef();
	const confirmPasswordRef = useRef();
	const [passwordResetDone, setPasswordResetDone] = useState(false);
	const [isInfoValid, setIsInfoValid] = useState(false);

	useEffect(() => document.title = "Verification • Instadicey", []);

	const handleVerifyEmail = async () =>
	{
		try
		{
			if (!email || !token)
				throw new Error("Email or token missing.");
			var result;
			if (localStorage.token) {
				result = await fetch(`${backend}/auth/confirm-email-change?newEmail=${encodeURIComponent(email)}&code=${encodeURIComponent(token)}`, {
					method: "POST",
					headers: {
						Authorization: `Bearer ${localStorage.token}`
					}
				});
				if (result.status === 401)
					return logOut(dispatch, history);
				await refreshOrLogout(dispatch, history);
			}
			else
				result = await fetch(`${backend}/auth/confirm-email?encodedEmail=${encodeURIComponent(email)}&code=${encodeURIComponent(token)}`, {method: "POST"});
			if (!result.ok) {
				const resultJSON = await result.json();
				throw new Error(resultJSON.detail, {cause: resultJSON.errors});
			}
			setStatus({
				type: "email-verification",
				ok: true,
			});
		} catch (err)
		{
			if (err.message.includes("Failed to fetch"))
				setErrorMsg("Server is down.");
			else
			{
				const errors = err.cause;
				if (!email || !token)
					setErrorMsg("Email or token missing.");
				else if (errors.length)
				{
					if (errors.some(e => e.code === "NotFound"))
						setErrorMsg(err.message);
					else if (errors.some(e => e.code === "InvalidToken"))
						setErrorMsg(err.message);
				}
				else
					setErrorMsg("An error has occurred.");	
			}
			setStatus({
				type: "email-verification",
				ok: false,
			});
		}
		dispatch(stopLoading());
	};

	const checkForm = () => checkPassword() && checkConfirmPassword() ? setIsInfoValid(true) : setIsInfoValid(false);
	const checkPassword = () => passwordRef.current.value.length < 6 ? false : true;
	const checkConfirmPassword = () => confirmPasswordRef.current.value === passwordRef.current.value ? true : false;
	const handleFormSubmitPassword = async e =>
	{
		e.preventDefault();
		if (!isInfoValid) return;

		const newPassword = passwordRef.current.value;
		try
		{
			const result = await fetch(`${backend}/auth/password-reset`, {
				method: "POST",
				headers: {
					"Content-Type": "application/json"
				},
				body: JSON.stringify({
					email,
					newPassword,
					token
				})
			});
			if (!result.ok) {
				const resultJSON = await result.json();
				throw new Error(resultJSON.detail);
			}
			setPasswordResetDone(true);
			setStatus({
				type: "password-reset",
				ok: true,
			});
		} catch (err)
		{
			setErrorMsg(err.message ?? "An error occurred. Please, try again later.");
			setStatus({
				type: "password-reset",
				ok: false,
			});
		}
	};

	const handlePasswordReset = async () =>
	{
		try
		{
			if (!email || !token)
				throw new Error("Email or token missing.");
			
			const tokenCheckResult = await fetch(`${backend}/auth/check-password-reset-token?email=${encodeURIComponent(email)}&token=${encodeURIComponent(token)}`, { method: "POST" });
			if (!tokenCheckResult.ok) throw new Error("Token is invalid or user was not found.");
		} catch (err) {
			setStatus({
				type: "password-reset",
				ok: false,
			});
			setErrorMsg(err.message ?? "Please try to reset the password again.");
		}
		dispatch(stopLoading());
	};

	const handleLink = async () =>
	{
		switch (mode)
		{
			case "verifyEmail":
				await handleVerifyEmail();
				break;

			case "resetPassword":
				await handlePasswordReset();
				break;

			default:
				break;
		}
	};

	useEffect(() =>
	{
		dispatch(startLoading());
		handleLink();
	}, [mode]);


	if (mode === "resetPassword" && !passwordResetDone && status.ok !== false)
	{
		return(
			<div className="verification-window outlined password-reset">
				<div className="icon"><img src={Logo} alt="logo"></img></div>
				<div className="email-div">
					<h2>Your Email:</h2>
					<span>{email}</span>
				</div>
				<form onSubmit={handleFormSubmitPassword}>
					<input type="password" id="password" placeholder="Password (at least 6 characters)" ref={passwordRef} onChange={checkForm}></input>
					<input type="password" id="confirm password" placeholder="Confirm Password" ref={confirmPasswordRef} onChange={checkForm}></input>
					<button className={`${isInfoValid ? null : "disabled"}`}>Reset Password</button>
				</form>
			</div>
		);
	} else if (status.ok)
	{
		return(
			<div className="verification-window success outlined">
				<div className="icon"><img src={greenCheckmark} alt="success"></img></div>
				{
					status.type === "email-verification"
						? 
						<div className="text-div">
							<h1>Your email address has been verified</h1>
							<p>Please go back to the sign up page to continue.</p>
						</div>
						: status.type === "password-reset"
							?
							<div className="text-div">
								<h1>Your password has been reset</h1>
								<p>You can now log in with the new password.</p>
							</div>
							: null
				}
			</div>
		);
	} else if (status.ok === false)
	{
		return(
			<div className="verification-window failed outlined">
				<div className="icon"><img src={redX} alt="failed"></img></div>
				{
					status.type === "email-verification"
						? 
						<div className="text-div">
							<h1>An error has occured</h1>
							<p>{errorMsg}</p>
						</div>
						: status.type === "password-reset"
							?
							<div className="text-div">
								<h1>An error has occured</h1>
								<p>{errorMsg}</p>
							</div>
							: null
				}
			</div>
		);
	}
	else return null;
};

export default AccountVerification;
