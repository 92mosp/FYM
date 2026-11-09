import type { Role } from '../types'

type Props = {
  roles: Role[]
  selected: number[]
  onChange: (selected: number[]) => void
  idPrefix?: string
}

export function RoleCheckboxes({ roles, selected, onChange, idPrefix = 'role' }: Props) {
  return (
    <>
      {roles.map((role) => (
        <label className="checkbox" key={role.id} htmlFor={`${idPrefix}-${role.id}`}>
          <input
            id={`${idPrefix}-${role.id}`}
            type="checkbox"
            checked={selected.includes(role.id)}
            onChange={(event) =>
              onChange(
                event.target.checked
                  ? [...selected, role.id]
                  : selected.filter((id) => id !== role.id),
              )
            }
          />
          {role.name}
        </label>
      ))}
    </>
  )
}
