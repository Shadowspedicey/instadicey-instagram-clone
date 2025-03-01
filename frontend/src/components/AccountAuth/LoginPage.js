import { useState, useRef, useEffect } from "react";
import { useDispatch } from "react-redux";
import { Link } from "react-router-dom";
import { startLoading, stopLoading } from "../../state/actions/isLoading";
import { setUser } from "../../state/actions/currentUser";
import ErrorMsg from "./ErrorMsg";
import nameLogo from "../../assets/namelogo.png";
import {ping} from "../../helpers.js";
import "./auth.css";
import { backend } from "../../config.js";
import jwt from "jsonwebtoken";

const LoginPage = () =>
{
	const emailRef = useRef();
	const passwordRef = useRef();
	const dispatch = useDispatch();

	const [isInfoValid, setInfoValid] = useState(false);
	const [errorMsg, setErrorMsg] = useState(null);

	useEffect(() => document.title = "Login • Instadicey", []);

	const checkEmail = () =>
	{
		const emailValue = emailRef.current.value;
		if (emailValue === "") return false;
		// eslint-disable-next-line no-control-regex
		else if (!emailValue.match(/(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|"(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21\x23-\x5b\x5d-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])*")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\[(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?|[a-z0-9-]*[a-z0-9]:(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21-\x5a\x53-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])+)\])/))
			return false;
		else return true;
	};

	const checkPassword = () =>
	{
		const passwordValue = passwordRef.current.value;
		return passwordValue.length < 6 ? false : true;
	};

	const checkForm = () =>
	{
		if (checkEmail() && checkPassword()) setInfoValid(true);
		else setInfoValid(false);
	};

	const handleSubmit = async  e =>
	{
		e.preventDefault();
		if (!isInfoValid) return;

		const email = emailRef.current.value;
		const password = passwordRef.current.value;

		try
		{
			dispatch(startLoading());

			if (!(await ping())) {
				setErrorMsg("Server is down.");
				return dispatch(stopLoading());
			}

			const result = await fetch(`${backend}/auth/login`, {
				method: "POST",
				body: JSON.stringify({
					email,
					password
				}),
				headers: {
					"Content-Type": "application/json"
				}
			});
			const resultJSON = await result.json();

			if (!result.ok)
				throw new Error(resultJSON.detail, {cause: resultJSON.errors});

			localStorage.setItem("token", resultJSON.token);
			localStorage.setItem("refreshToken", resultJSON.refreshToken);
			const claims = jwt.decode(resultJSON.token);
			dispatch(setUser(claims));

			dispatch(stopLoading());
		} catch (err)
		{
			const errors = err.cause ?? [];
			if (errors.some(e => e.code === "UserNotFound"))
				setErrorMsg(e.description);
			else if (errors.some(e => e.code === "EmailNotVerified")) {
				const sendEmail = async () =>
				{
					await fetch(`${backend}/auth/send-email-verification?email=${encodeURIComponent(email)}`, {method: "POST"});
					setErrorMsg("Email sent");
				};
				setErrorMsg(<div>Email not verified<br/><span className="link" onClick={sendEmail}>click here</span> to send verification email</div>);
			} else
				setErrorMsg(err.message);

			dispatch(stopLoading());
		}
	};

	useEffect(() => dispatch(stopLoading()), [dispatch]);

	return(
		<div id="login-page">
			<div id="login-window" className="outlined">
				<div className="logo"><img src={nameLogo} alt="Instadicey logo"></img></div>
				{ errorMsg ? <ErrorMsg text={errorMsg}/> : null }
				<form className="info" onSubmit={handleSubmit}>
					<input type="text" placeholder="Email" id="email" ref={emailRef} onChange={checkForm}></input>
					<input type="password" placeholder="Password" id="password" ref={passwordRef} onChange={checkForm}></input>
					<button id="login" className={`${isInfoValid ? null : "disabled"}`}>Log In</button>
				</form>
				<Link to="/accounts/password-reset">Forgot password?</Link>
			</div>
			<div className="extra outlined"><span>Don't have an account? <Link to="/accounts/email-signup" className="button">Sign Up</Link></span></div>
		</div>
	);
};

export default LoginPage;
