import { Link, useNavigate } from "react-router-dom";
import { useForm } from "react-hook-form";
import { useState } from "react";
import Navbar from "../components/Navbar";
import "./Auth.css";
import { beeApi } from "../api/beeApi";
import { useAppContext } from "../context/AppContext";

function LoginPage() {
  const navigate = useNavigate();
  const { setSession } = useAppContext();
  const [loginError, setLoginError] = useState("");

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm();

  const onSubmit = async (data) => {
    setLoginError("");

    try {
      const response = await beeApi.post("/auth/login", {
        login: data.login,
        password: data.password,
      });
      setSession(response.data);
      navigate("/");
    } catch (requestError) {
      setLoginError(
        requestError?.response?.data?.message ||
          "Nieprawidłowy login lub hasło"
      );
    }
  };

  return (
    <>
      <Navbar />

      <div className="auth-page">
        <div className="auth-card">
          <div className="auth-top">
            <h2>Logowanie</h2>
            <p>Zaloguj się do BeeManager</p>
          </div>

          <form className="auth-form" onSubmit={handleSubmit(onSubmit)}>
            <label>Login lub e-mail</label>
            <input
              type="text"
              placeholder="twoj@email.com lub login"
              {...register("login", {
                required: "Login jest wymagany",
              })}
            />
            {errors.login && <p className="form-error">{errors.login.message}</p>}

            <label>Hasło</label>
            <input
              type="password"
              placeholder="Wpisz hasło"
              {...register("password", {
                required: "Hasło jest wymagane",
                minLength: {
                  value: 5,
                  message: "Hasło musi mieć minimum 5 znaków",
                },
              })}
            />
            {errors.password && (
              <p className="form-error">{errors.password.message}</p>
            )}

            <button type="submit" className="auth-submit">
              Zaloguj się
            </button>

            {loginError && <p className="form-error">{loginError}</p>}
          </form>

          <p className="auth-switch">
            Nie masz konta? <Link to="/register">Zarejestruj się</Link>
          </p>

          <p className="auth-back">
            <Link to="/">← Wróć na stronę główną</Link>
          </p>
        </div>
      </div>
    </>
  );
}

export default LoginPage;

