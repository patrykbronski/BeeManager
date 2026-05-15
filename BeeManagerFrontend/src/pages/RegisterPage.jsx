import { Link, useNavigate } from "react-router-dom";
import { useForm } from "react-hook-form";
import { useState } from "react";
import Navbar from "../components/Navbar";
import "./Auth.css";
import { beeApi } from "../api/beeApi";
import { requestedRoleOptions } from "../utils/beeEnums";

function RegisterPage() {
  const navigate = useNavigate();
  const [registerError, setRegisterError] = useState("");

  const {
    register,
    handleSubmit,
    watch,
    formState: { errors },
  } = useForm({
    defaultValues: {
      requestedRole: "Owner",
    },
  });

  const password = watch("password");

  const onSubmit = async (data) => {
    setRegisterError("");

    try {
      await beeApi.post("/auth/register", {
        fullName: data.fullName,
        email: data.email,
        password: data.password,
        requestedRole: data.requestedRole,
        registrationNote: data.registrationNote || "",
      });
      navigate("/login");
    } catch (requestError) {
      setRegisterError(
        requestError?.response?.data?.message ||
          "Nie udało się utworzyć konta"
      );
    }
  };

  return (
    <>
      <Navbar />

      <div className="auth-page">
        <div className="auth-card">
          <div className="auth-top">
            <h2>Rejestracja</h2>
            <p>Załóż konto w BeeManager</p>
          </div>

          <form className="auth-form" onSubmit={handleSubmit(onSubmit)}>
            <label>Imię i nazwisko</label>
            <input
              type="text"
              placeholder="Jan Kowalski"
              {...register("fullName", {
                required: "Imię i nazwisko jest wymagane",
                minLength: {
                  value: 3,
                  message: "Imię i nazwisko musi mieć minimum 3 znaki",
                },
              })}
            />
            {errors.fullName && (
              <p className="form-error">{errors.fullName.message}</p>
            )}

            <label>Adres e-mail</label>
            <input
              type="email"
              placeholder="twoj@email.com"
              {...register("email", {
                required: "Adres e-mail jest wymagany",
                pattern: {
                  value: /^[^\s@]+@[^\s@]+\.[^\s@]+$/,
                  message: "Podaj poprawny adres e-mail",
                },
              })}
            />
            {errors.email && (
              <p className="form-error">{errors.email.message}</p>
            )}

            <label>Rola</label>
            <select {...register("requestedRole", { required: true })}>
              {requestedRoleOptions.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>

            <label>Notatka do rejestracji</label>
            <textarea
              placeholder="Kilka słów o sobie lub o planowanej pracy"
              {...register("registrationNote")}
            />

            <label>Hasło</label>
            <input
              type="password"
              placeholder="Ustaw hasło"
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

            <label>Powtórz hasło</label>
            <input
              type="password"
              placeholder="Powtórz hasło"
              {...register("repeatPassword", {
                required: "Powtórzenie hasła jest wymagane",
                validate: (value) =>
                  value === password || "Hasła muszą być takie same",
              })}
            />
            {errors.repeatPassword && (
              <p className="form-error">{errors.repeatPassword.message}</p>
            )}

            <button type="submit" className="auth-submit">
              Zarejestruj się
            </button>

            {registerError && <p className="form-error">{registerError}</p>}
          </form>

          <p className="auth-switch">
            Masz już konto? <Link to="/login">Zaloguj się</Link>
          </p>

          <p className="auth-back">
            <Link to="/">← Wróć na stronę główną</Link>
          </p>
        </div>
      </div>
    </>
  );
}

export default RegisterPage;

